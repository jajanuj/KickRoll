# KickRoll — 點名開踢 ⚽

> **10 秒內完成一堂課的點名，扣課自動化，報表一鍵匯出**

---

## 📱 App 基本資訊
- **中文顯示名（商店）**：點名開踢
- **英文產品名**：KickRoll
- **Solution 名稱**：KickRoll
- **命名空間 / 專案名稱**：KickRoll.App
- **平台**：.NET MAUI (前端)、ASP.NET Core / Firebase (後端選項)

---

## 🎯 目標
- 教練在 **10 秒內完成一堂課的點名**。
- 出勤與扣課自動同步，誤差率 < 1%。
- 報表一鍵匯出，方便對帳與行政作業。

---

## 🚀 MVP 功能
- **教練端**
  - 今日課表清單、一鍵全選點名
  - 清單勾選點名、QR Code 簽到
  - 課後補登 / 修正
- **管理員**
  - 建立隊別與課表
  - 銷售方案（次數制 / 期限制）
  - 手動收款紀錄（現金 / 轉帳 / 其他）
  - 報表匯出（出勤、扣課、庫存）
- **會員 / 家長**
  - 查看課表與剩餘堂數
  - 出示 QR 簽到碼
  - 線上請假（依規則扣堂）

---

## 🗂️ 專案結構建議
```
KickRoll/
├── README.md
├── docs/        # 規格、設計、UAT 測試情境
├── src/
│   └── KickRoll.App/   # .NET MAUI 前端 App
│   └── KickRoll.Api/   # ASP.NET Core 後端 (可選)
├── infra/      # 部署、Firebase 設定、CI/CD pipeline
└── design/     # Wireframe、UI 設計
```

---

## 🔒 技術選型
- **前端**：.NET MAUI (跨平台 App)
- **後端**：ASP.NET Core API / Firebase Firestore
- **資料儲存**：Firestore（推薦），或 Supabase（等價結構）
- **推播**：Firebase Cloud Messaging
- **登入與安全**：Firebase Auth + Firestore Rules

---

## 📊 報表格式
- 出勤報表：`date, team, sessionId, memberId, memberName, status`
- 扣課報表：`date, memberId, packageId, amount, reason`
- 庫存報表：`memberId, packageName, remainingCount, expireAt`

---

## 🛠️ 開發里程碑
- **M1 (2 週)**：資料模型、Admin 建班 / 課表、教練清單點名
- **M2 (2 週)**：QR 簽到、離線同步、自動扣課
- **M3 (1 週)**：報表匯出、推播
- **M4 (1 週)**：UAT 測試、上架、隱私政策

---

## 📌 後續 Backlog
- 家長端支援多孩切換
- 教練績效儀表板
- 金流整合（綠界 / Stripe）
- 地理圍欄自動簽到

---

## 👥 團隊
- Product / Spec:  
- Development:  
- Design:  

（可依團隊實際情況補上）
