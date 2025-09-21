# 足球會員點名 App — MVP 規格草案 v1.0

> 目標：讓教練「10 秒內完成一堂課的點名」，並自動、可追蹤地扣課與出報表。

### 本版更新（V1）
- MVP 保持**手動收款**，僅記錄與對帳；線上購買移至後續功能追加。
- 新增 **Admin 手動收款** User Story 與資料欄位（`payments.method='manual'`、`channel`、`note`、`attachmentUrl`）。
- 技術選型加入 **.NET MAUI** 路線（C# 團隊友善），補充 ASP.NET Core 後端選項與背景工作方案。

---

## 1) 角色與目標
- **Admin（管理員）**：建班、建課表、管理會員與方案、匯出報表。
- **Coach（教練）**：查看今日課表、快速點名（清單或掃碼）、課後補登/修正。
- **Member / Parent（學員／家長）**：看課表、掃碼簽到、請假/補課、查看剩餘堂數。

KPI：
1. 每堂點名完成時間 ≤ 10 秒（不含大班掃碼時間）。
2. 出勤與扣課錯誤率 < 1%。
3. 報表一鍵匯出（CSV/Excel）。

---

## 2) 典型流程（User Journey）
1. Admin 建立 **隊別（Team）** 與 **課表（Class Sessions）**。
2. Admin 記錄**手動收款**並指派 **課程方案（Package）**（次數制或期限制；線上購買待 V2）。
3. 開課前推播提醒（T-24h / T-2h）。
4. 現場點名：
   - 教練開啟「本堂點名」頁 → 一鍵全選已報名名單，逐一調整；或
   - 掃 QR Code（學員 App 出示／紙本），系統即時標記出席並去重。
5. 下課：自動寫入 **出勤紀錄** + **扣課紀錄**。
6. 異常處理：請假（是否扣堂）、遲到、補登。
7. 週/月報表匯出，低餘額自動通知。

---

## 3) User Stories（MVP 範圍）
$1- 建立方案（次數包 / 月票）、售出並指派給會員。
- **記錄手動收款**（金額、日期、經手人、備註、上傳憑證）與對帳報表；線上購買 V2 再開。
- 查看報表：出勤率、剩餘堂數、扣課明細、教練堂數統計。

### Coach
- **[必備]** 今日課表清單，進入某堂課→快速點名。
- **[必備]** 清單點名（勾選 present/late/absent/excused）。
- **[選用]** QR 掃碼點名（線上/離線），重複掃碼自動忽略。
- 課後補登與修正（寫入稽核日誌）。

### Member / Parent
- 查看我的課表與剩餘堂數。
- 出示 QR 簽到碼。
- 請假申請（限制時間與是否扣堂）。

---

## 4) 規則（MVP 版）
- **請假規則**：
  - 課前 X 小時（預設 6 小時）前請假 → 不扣堂；逾時請假 → 扣 1 堂。
  - 期限票（如月票）逾時請假不影響剩餘堂數，但記錄缺席。
- **遲到**：課後 Y 分鐘（預設 10 分）內補簽 → 視為出席；超過則標遲到（是否扣堂由方案設定）。
- **補課**：缺席可登記候補其他班，是否補扣由 Admin 設定（MVP 可先不自動匹配）。
- **去重**：同一 memberId + sessionId 只允許一筆 `present/late` 有效紀錄。
- **候補**：名額滿 → 加入 waiting list，Admin/Coach 一鍵轉正（廣播通知）。

---

- **payments**（MVP：手動收款登錄）
  - `paymentId`, `memberId`, `memberPackageId`, `amount`, `paidAt`
  - `method` (enum: manual), `channel` (cash|bank|transfer|other)
  - `operatorUserId`（經手人）, `note`, `attachmentUrl`（收據/轉帳截圖）
  - （V2 才加入：gateway, txRef, status, refundedAt）

### 範例文件 資料結構（Firestore 建議）
> 命名以複數集合為主；必要時建立複合索引。

### Collections & Fields
- **users**（僅登入帳號與角色）
  - `uid` (string, docId)
  - `role` (enum: admin|coach|member|parent)
  - `displayName`, `phone`, `email`
  - `memberId` (ref to members, for member/parent)

- **members**（會員主檔）
  - `memberId` (docId)
  - `name`, `birthdate`, `gender`, `phone`, `parentContacts[]`
  - `teamIds[]`（所屬隊別）
  - `status` (active|paused|left)

- **teams**
  - `teamId` (docId)
  - `name`, `location`, `capacity`, `coachIds[]`
  - `scheduleHints`（例：每週二四 18:00）

- **class_sessions**（單堂課）
  - `sessionId` (docId)
  - `teamId`, `coachIds[]`
  - `startAt` (timestamp), `endAt` (timestamp)
  - `location`, `capacity`, `status` (scheduled|canceled|done)
  - `waitlist[]`（memberIds）

- **attendances**（出勤紀錄：高頻寫入，單獨集合）
  - `attendanceId` (docId)
  - `sessionId`, `memberId`
  - `status` (present|late|absent|excused)
  - `checkInAt` (timestamp), `method` (qr|manual|import)
  - `createdBy`, `updatedBy`, `auditLog[]`
  - **索引**：`sessionId+status`, `memberId+checkInAt desc`

- **packages**（方案定義）
  - `packageId` (docId)
  - `name`, `type` (count|period)
  - `count`（次數；type=count）
  - `periodDays`（天數；type=period）
  - `latePolicy`（遲到是否扣堂）

- **member_packages**（會員購買方案，避免在 members 內嵌）
  - `memberPackageId` (docId)
  - `memberId`, `packageId`
  - `remainingCount`（type=count）
  - `startAt`, `expireAt`（type=period）
  - `status` (active|expired|frozen)

- **deductions**（扣課紀錄）
  - `deductionId` (docId)
  - `attendanceId`, `memberPackageId`
  - `amount` (int; 通常 1)
  - `reason` (auto|manual|late|no-show|other)
  - `createdAt`

- **payments**（若開金流）
  - `paymentId`, `memberId`, `memberPackageId`, `amount`, `method`, `paidAt`, `txRef`

### 範例文件（JSON）
```json
// attendances/{attendanceId}
{
  "sessionId": "S_2025_09_20_T1_1800",
  "memberId": "M_000123",
  "status": "present",
  "checkInAt": 1758295200000,
  "method": "qr",
  "createdBy": "coach_01"
}
```

---

## 7) QR 簽到與離線策略
- **QR 內容格式**：`clubId|sessionId|memberId|exp|nonce|sig`
  - `sig` 為後端以私鑰簽名（或 HMAC）→ App 無網亦可做基本驗章。
- **掃碼流程**：
  1) 教練端掃碼 → 驗簽成功 → 先寫本地 Queue（離線）並標記畫面「已簽」。
  2) 線上時：背景批次同步到 Firestore；
     - 使用 `sessionId+memberId` 作為 **自然鍵** 去重（Cloud Function 以 transaction/upsert）。
- **防重與回寫**：
  - Cloud Function 成功寫入→ 回傳成功；若重複→ 回傳 200 + `duplicate=true`，前端只顯示「已簽過」。

---

## 8) 扣課引擎（Cloud Function 伺服端）
事件驅動：當 `attendances` 新增/更新為出席狀態時觸發。

Pseudo-code：
```ts
onWrite(attendance):
  if status in ["present","late"]:
    pkg = pickActivePackage(memberId)
    if pkg.type == "count":
      if not alreadyDeducted(attendanceId):
        write deduction(amount=1)
        decrement(pkg.remainingCount)
  elif status in ["absent"]:
    if lateCancel(attendance):
      maybeDeductAccordingToPolicy()
```

挑選方案策略 `pickActivePackage`：
1. 先找 `status=active`，到期日最近者；
2. 若多個次數包並存 → 先扣 **到期日最近** 或 **購買舊** 的方案。

---

## 9) 安全與權限（Firestore Security Rules 草案）
```js
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    function isSignedIn() { return request.auth != null; }
    function isRole(r) { return isSignedIn() && request.auth.token.role == r; }

    match /users/{uid} {
      allow read: if isSignedIn() && (request.auth.uid == uid || isRole('admin'));
      allow write: if isRole('admin');
    }

    match /members/{id} {
      allow read: if isRole('admin') || isRole('coach');
      allow write: if isRole('admin');
    }

    match /teams/{id} {
      allow read: if isSignedIn();
      allow write: if isRole('admin');
    }

    match /class_sessions/{id} {
      allow read: if isSignedIn();
      allow write: if isRole('admin');
    }

    match /attendances/{id} {
      allow read: if isRole('admin') || isRole('coach');
      allow create, update: if isRole('coach') || isRole('admin');
    }

    match /member_packages/{id} {
      allow read: if isRole('admin') || isRole('coach');
      allow write: if isRole('admin');
    }

    match /deductions/{id} {
      allow read: if isRole('admin');
      allow write: if isRole('admin') || isRole('function'); // 由 CF 服務帳號寫入
    }
  }
}
```

---

## 10) 介面 Wireframes（文字版）
### A. 教練端 — 今日課表
```
[09/20 (六)] 18:00–19:30  U12 甲組 @ 市民運動中心  [進入點名]
[09/20 (六)] 19:45–21:15  U15 乙組 @ 市民運動中心  [進入點名]
```

### B. 教練端 — 點名頁（10 秒流）
```
U12 甲組 18:00–19:30  |  現場人數( )  搜尋🔎  掃碼📷
----------------------------------------------------
☑ 王小明  (剩 5 堂)
☑ 陳小美  (剩 2 堂)  ⚠ 低餘額
□ 李大雄  (剩 8 堂)
…
[全選出席]  [全設缺席]  [完成]
```
- 操作：預設全選出席，一鍵「完成」→ 寫入出勤 + 扣課。

### C. 會員端 — 我的課
```
本週課表
- 09/20(六) 18:00 U12 甲組  [出示 QR]
- 09/22(一) 18:00 體能班     [請假]
剩餘堂數：5  （低於 3 堂將提醒）
```

---

## 11) 通知（Push）
- T-24h / T-2h 課前提醒。
- 點名完成後，家長收到「本堂出勤：出席/遲到/缺席」。
- 低於門檻（預設 3 堂）提醒續購。

---

## 12) 報表（CSV 欄位建議）
- **出勤報表**：date, team, sessionId, memberId, memberName, status, method, checkInAt
- **扣課報表**：date, memberId, packageId, amount, reason
- **庫存報表**：memberId, packageName, remainingCount, expireAt

---

## 13) 測試情境（UAT 清單）
- **請假**：課前 8h / 2h 提交，是否正確扣課。
- **離線掃碼**：關網 30 秒內掃 5 人 → 回到線上是否無重複、順序無關。
- **重複簽到**：同一人掃兩次 → 僅一筆有效出勤。
- **滿班候補**：候補→轉正→推播。
- **多方案並存**：同會員有兩個次數包，是否扣對優先順序。

---

## 14) 風險與備援
- **裝置離線**：本地 Queue + 指紋去重（`sessionId+memberId`）。
- **教練臨時換人**：同隊 `coachIds[]` 授權；突發由 Admin 遠端開權。
- **法規/個資**：最小化個資顯示（名單只顯示名與末三碼），導出報表需授權。

---

## 15) 里程碑（建議切分）
- **M1（2 週）**：資料模型 + Admin 建班/課表 + 教練點名清單流。
- **M2（2 週）**：QR 簽到 + 離線同步 + 自動扣課。
- **M3（1 週）**：報表匯出 + 推播。
- **M4（1 週）**：UAT + 上架素材 + 隱私政策頁。

---

## 16) 後續 Backlog
- 家長端支援多孩切換。
- 教練績效儀表板、學員出勤熱力圖。
- 金流（綠界/Stripe）與電子收據。
- 地理圍欄自動簽到（到場半徑 50m 內）。

---

### 備註
- 若選 **Supabase**：等價資料表為 `users, members, teams, class_sessions, attendances, packages, member_packages, deductions, payments`，以外鍵+唯一索引（`UNIQUE (session_id, member_id)`）去重，RLS 以角色控制行級存取。

