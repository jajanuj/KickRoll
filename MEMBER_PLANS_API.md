# 會員方案 API 端點

## 概述
本文件說明已實現的會員方案 API 端點，遵循規範要求。

## API 端點

### 1. 建立會員方案
**POST** `/api/members/{memberId}/plans`

請求主體 (camelCase JSON):
```json
{
  "type": "credit_pack",
  "name": "10堂計次課程",
  "totalCredits": 10,
  "remainingCredits": 10,
  "validFrom": "2024-01-01T00:00:00Z",
  "validUntil": "2024-06-30T23:59:59Z",
  "status": "active"
}
```

回應:
```json
{
  "success": true,
  "plan": {
    "id": "generated-plan-id",
    "type": "credit_pack",
    "name": "10堂課程包",
    "totalCredits": 10,
    "remainingCredits": 10,
    "validFrom": "2024-01-01T00:00:00Z",
    "validUntil": "2024-06-30T23:59:59Z",
    "status": "active",
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T10:30:00Z"
  }
}
```

### 2. 查詢會員方案
**GET** `/api/members/{memberId}/plans?status={status}`

查詢參數:
- `status` (可選): "active"(啟用), "expired"(過期), "suspended"(暫停)

回應:
```json
[
  {
    "id": "plan-id-1",
    "type": "credit_pack",
    "name": "10堂課程包",
    "totalCredits": 10,
    "remainingCredits": 8,
    "validFrom": "2024-01-01T00:00:00Z",
    "validUntil": "2024-06-30T23:59:59Z",
    "status": "active",
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-20T14:15:00Z"
  },
  {
    "id": "plan-id-2",
    "type": "time_pass",
    "name": "包月課程",
    "totalCredits": null,
    "remainingCredits": 0,
    "validFrom": "2024-01-01T00:00:00Z",
    "validUntil": "2024-01-31T23:59:59Z",
    "status": "active",
    "createdAt": "2024-01-10T09:00:00Z",
    "updatedAt": "2024-01-10T09:00:00Z"
  }
]
```

### 3. 更新會員方案
**PATCH** `/api/members/{memberId}/plans/{planId}`

請求主體 (僅包含要更新的欄位):
```json
{
  "remainingCredits": 5,
  "status": "active"
}
```

回應:
```json
{
  "success": true,
  "message": "方案更新成功"
}
```

### 4. 調整方案堂數
**POST** `/api/members/{memberId}/plans/{planId}:adjust`

請求主體:
```json
{
  "delta": -1,
  "reason": "上課出席"
}
```

回應:
```json
{
  "success": true,
  "message": "堂數調整成功",
  "newRemainingCredits": 7,
  "delta": -1
}
```

## Firestore 資料結構

資料以 PascalCase 欄位名稱儲存在 Firestore：

**集合路徑**: `members/{MemberId}/plans/{PlanId}`

文件欄位 (Firestore 中使用 PascalCase):
- `Type`: "credit_pack" | "time_pass"
- `Name`: 字串
- `TotalCredits`: 數字 (time_pass 可為 null)
- `RemainingCredits`: 數字  
- `ValidFrom`: 時間戳記 (可為 null)
- `ValidUntil`: 時間戳記 (time_pass 必填；credit_pack 可為 null)
- `Status`: "active"(啟用) | "expired"(過期) | "suspended"(暫停)
- `CreatedAt`: 建立時間戳記
- `UpdatedAt`: 更新時間戳記

## 實現的商業規則

1. **RemainingCredits ≥ 0**: 剩餘堂數不可為負數
2. **Type 驗證**: 必須為 "credit_pack" 或 "time_pass"
3. **Status 驗證**: 必須為 "active"、"expired" 或 "suspended"
4. **time_pass 需要 ValidUntil**: 期限票必須有到期日期
5. **自動過期**: 當 ValidUntil 超過當前時間時，方案自動標記為 "expired"
6. **MergeAll 更新**: 使用 Firestore SetOptions.MergeAll 避免覆蓋現有欄位

## 功能特色

- **PascalCase Firestore 欄位**: 所有 Firestore 文件欄位使用 PascalCase 命名
- **camelCase API**: JSON 請求/回應使用 camelCase 保持外部 API 一致性
- **FirestoreProperty 對應**: 使用 `[FirestoreProperty("PascalName")]` 進行正確的欄位對應
- **完整驗證**: 所有操作都有商業規則驗證
- **自動過期檢查**: 查詢時自動更新過期方案
- **彈性更新**: PATCH 端點僅更新提供的欄位
- **堂數調整**: 專用端點進行正負堂數調整