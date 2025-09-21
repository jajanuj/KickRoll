# Firestore 初始化操作手冊

## 🎯 目標
在 Firebase 專案中啟用 Firestore，並準備好與 `seed.js` 搭配的 Service Account 金鑰。

---

## 1. 建立 Firestore Database
1. 登入 [Firebase Console](https://console.firebase.google.com/)  
2. 選擇專案 **KickRoll (kickroll-7250e)**  
3. 左側選單 → **Firestore Database**  
4. 點擊 **建立資料庫 (Create Database)**  
5. 在第一步「選取版本」：
   - 建議選 **Standard 版**（預設選項，適合一般 App 使用，文件上限 1MB）。
   - **Enterprise 版** 主要針對 MongoDB 相關需求，本專案不需要。  
   → 點擊 **下一步**。
6. 在第二步「Database ID & location」：
   - **Database ID**：保持預設 `(default)` 即可。
   - **位置 (Region)**：建議選 **asia-east1**（亞洲東部，台灣/日本/香港延遲最低）。
   → 點擊 **啟用**。
7. 在第三步「設定」會要求選擇 **Security Rules**：
   - 開發階段可先選 **測試模式 (Test mode)**（允許所有讀寫，方便快速開發）。
   - 上線前必須改成 **鎖定模式 (Production mode)**，並套用專案定義的 **安全規則**（例如僅 admin/coach 可以寫入出勤紀錄）。
8. 完成後，Console 會出現一個空的 Firestore Database。

---

## 2. 產生 Service Account 金鑰
1. Firebase Console → 左上角齒輪 → **專案設定 (Project Settings)**  
2. 切換到 **服務帳號 (Service accounts)** 分頁  
3. 點擊 **產生新的私密金鑰 (Generate new private key)**  
4. 系統會下載一個 JSON 檔案，例如：
   ```
   kickroll-7250e-firebase-adminsdk-xxxx.json
   ```
5. 將檔案重新命名為 **serviceAccount.json**，放到 `ImportFirestore/` 資料夾下。

---

## 3. 匯入測試資料
1. 確保以下檔案在 `ImportFirestore/` 目錄下：
   ```
   serviceAccount.json
   firestore_seed.json
   seed_fixed.js
   ```
2. 在 PowerShell 執行：
   ```powershell
   node seed_fixed.js
   ```
3. 如果成功，會看到：
   ```
   ✅ Imported users/admin_01
   ✅ Imported members/M_0001
   ...
   🎉 All data imported!
   ```

---

## 4. 驗證
- 打開 Firebase Console → **Firestore Database → Data**  
- 應該會看到剛剛的集合：
  - `users`
  - `members`
  - `teams`
  - `class_sessions`
  - `packages`
  - `member_packages`

---

## 5. 常見錯誤排除
- **`SERVICE_DISABLED`**  
  → Firestore API 沒啟用。請到 [GCP Console Firestore API](https://console.developers.google.com/apis/api/firestore.googleapis.com/) 按「啟用」。  

- **`NOT_FOUND`**  
  → Firestore Database 還沒建立。請回到 Firebase Console → Firestore → 建立資料庫。  

- **`PERMISSION_DENIED`**  
  → `serviceAccount.json` 金鑰錯誤，請重新下載並確認 `project_id` 正確。  

---

## 6. Security Rules 範例

```js
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    function isSignedIn() { return request.auth != null; }
    function hasRole(r) { return isSignedIn() && request.auth.token.role == r; }

    // 使用者帳號
    match /users/{uid} {
      allow read: if isSignedIn() && (request.auth.uid == uid || hasRole("admin"));
      allow write: if hasRole("admin");
    }

    // 會員主檔
    match /members/{id} {
      allow read: if hasRole("admin") || hasRole("coach");
      allow write: if hasRole("admin");
    }

    // 隊別
    match /teams/{id} {
      allow read: if isSignedIn();
      allow write: if hasRole("admin");
    }

    // 課表
    match /class_sessions/{id} {
      allow read: if isSignedIn();
      allow write: if hasRole("admin");
    }

    // 出勤紀錄
    match /attendances/{id} {
      allow read: if hasRole("admin") || hasRole("coach");
      allow create, update: if hasRole("coach") || hasRole("admin");
    }

    // 會員方案
    match /member_packages/{id} {
      allow read: if hasRole("admin") || hasRole("coach");
      allow write: if hasRole("admin");
    }

    // 扣課紀錄（由系統函式寫入）
    match /deductions/{id} {
      allow read: if hasRole("admin");
      allow write: if hasRole("admin") || request.auth.token.role == "function";
    }

    // 收款紀錄
    match /payments/{id} {
      allow read: if hasRole("admin");
      allow write: if hasRole("admin");
    }
  }
}
```
