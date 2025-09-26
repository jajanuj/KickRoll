# 成員報名課程（Enrollment）功能說明

## 功能概述

本實作為 KickRoll 系統新增了完整的成員報名課程功能，包含 API 端點、Firestore 資料結構和 App 界面。

## API 端點

### 1. 報名課程
**POST** `/api/sessions/{sessionId}/enroll`

請求主體 (camelCase JSON):
```json
{
  "memberId": "member-123"
}
```

回應:
```json
{
  "success": true,
  "enrollment": {
    "id": "member-123",
    "memberId": "member-123",
    "sessionId": "session-456",
    "status": "enrolled",
    "createdAt": "2024-01-15T10:30:00Z"
  }
}
```

### 2. 取消報名
**POST** `/api/sessions/{sessionId}/cancel`

請求主體和回應格式與報名相同，但 status 為 "cancelled"。

### 3. 查詢會員報名紀錄
**GET** `/api/members/{memberId}/enrollments?from={from}&to={to}`

查詢參數:
- `from` (可選): 開始時間 (ISO 8601)
- `to` (可選): 結束時間 (ISO 8601)

回應:
```json
[
  {
    "enrollmentId": "enrollment-123",
    "sessionId": "session-456",
    "status": "enrolled",
    "createdAt": "2024-01-15T10:30:00Z",
    "session": {
      "id": "session-456",
      "courseId": "course-789",
      "teamId": "team-abc",
      "startTime": "2024-01-20T14:00:00Z",
      "endTime": "2024-01-20T15:30:00Z",
      "capacity": 20,
      "enrolledCount": 15,
      "createdAt": "2024-01-10T09:00:00Z",
      "updatedAt": "2024-01-15T10:30:00Z"
    }
  }
]
```

## Firestore 資料結構

### Sessions 集合
```
sessions/{SessionId}
├── CourseId: string
├── TeamId: string
├── StartTime: Timestamp
├── EndTime: Timestamp
├── Capacity: number
├── EnrolledCount: number
├── CreatedAt: Timestamp
└── UpdatedAt: Timestamp
```

### Enrollments 子集合
```
sessions/{SessionId}/enrollments/{MemberId}
├── MemberId: string
├── SessionId: string
├── Status: "enrolled" | "cancelled"
└── CreatedAt: Timestamp
```

## 核心功能特色

### 1. 交易式報名邏輯
- 使用 Firestore 交易確保 `EnrolledCount` 原子性更新
- 防止超賣問題（容量控制）
- 幂等性支援（重複報名會被攔截）

### 2. 命名規範
- **Firestore 欄位**: PascalCase（如 `MemberId`, `StartTime`）
- **API JSON**: camelCase（如 `memberId`, `startTime`）
- **模型對應**: 使用 `[FirestoreProperty("PascalName")]` 屬性

### 3. 錯誤處理
- **capacity_full**: 課程容量已滿
- **already_enrolled**: 該會員已報名此課程
- **enrollment_not_found**: 找不到報名紀錄（取消報名時）
- **session_not_found**: 找不到課程場次

## App 功能

### 1. 課表列表頁面
- 顯示每個場次的報名人數/容量（如：報名人數：15/20）
- 點擊場次進入詳情頁面

### 2. 課程詳情頁面
- 報名功能：輸入會員 ID 進行報名
- 取消報名功能：取消指定會員的報名
- 即時錯誤回饋（容量滿、重複報名等）

### 3. 會員報名紀錄頁面
- 查看個別會員的所有報名紀錄
- 支援日期篩選
- 顯示報名狀態（enrolled/cancelled）
- 包含完整的課程資訊

### 4. 會員列表頁面更新
- 新增"查看報名"按鈕，可快速查看該會員的報名紀錄
- 保留原有的編輯和方案管理功能

## 測試覆蓋

已新增 11 個單元測試，涵蓋：
- DTO 序列化/反序列化
- 容量檢查邏輯
- 錯誤處理機制
- 報名狀態驗證
- 自定義異常測試

所有測試均通過（總計 84 個測試）。

## 使用方法

1. 啟動 API 服務器
2. 在 App 中選擇課程場次
3. 輸入會員 ID 進行報名或取消報名
4. 透過會員列表查看個別會員的報名紀錄

系統會自動處理容量控制、重複報名檢查和資料一致性維護。