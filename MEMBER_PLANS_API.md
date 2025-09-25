# Member Plans API Endpoints

## Overview
This document demonstrates the Member Plans API endpoints that have been implemented according to the specifications.

## Endpoints

### 1. Create Member Plan
**POST** `/api/members/{memberId}/plans`

Request body (camelCase JSON):
```json
{
  "type": "credit_pack",
  "name": "10 Class Pack",
  "totalCredits": 10,
  "remainingCredits": 10,
  "validFrom": "2024-01-01T00:00:00Z",
  "validUntil": "2024-06-30T23:59:59Z",
  "status": "active"
}
```

Response:
```json
{
  "success": true,
  "plan": {
    "id": "generated-plan-id",
    "type": "credit_pack",
    "name": "10 Class Pack",
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

### 2. Get Member Plans
**GET** `/api/members/{memberId}/plans?status={status}`

Query parameters:
- `status` (optional): "active", "expired", "suspended"

Response:
```json
[
  {
    "id": "plan-id-1",
    "type": "credit_pack",
    "name": "10 Class Pack",
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
    "name": "Monthly Pass",
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

### 3. Update Member Plan
**PATCH** `/api/members/{memberId}/plans/{planId}`

Request body (only include fields to update):
```json
{
  "remainingCredits": 5,
  "status": "active"
}
```

Response:
```json
{
  "success": true,
  "message": "Plan updated successfully"
}
```

### 4. Adjust Plan Credits
**POST** `/api/members/{memberId}/plans/{planId}:adjust`

Request body:
```json
{
  "delta": -1,
  "reason": "Class attended"
}
```

Response:
```json
{
  "success": true,
  "message": "Credits adjusted successfully",
  "newRemainingCredits": 7,
  "delta": -1
}
```

## Firestore Structure

The data is stored in Firestore with PascalCase field names:

**Collection**: `members/{MemberId}/plans/{PlanId}`

Document fields (PascalCase in Firestore):
- `Type`: "credit_pack" | "time_pass"
- `Name`: string
- `TotalCredits`: number (can be null for time_pass)
- `RemainingCredits`: number  
- `ValidFrom`: Timestamp (can be null)
- `ValidUntil`: Timestamp (required for time_pass, can be null for credit_pack)
- `Status`: "active" | "expired" | "suspended"
- `CreatedAt`: Timestamp
- `UpdatedAt`: Timestamp

## Business Rules Implemented

1. **RemainingCredits ≥ 0**: Cannot go negative
2. **Type validation**: Must be "credit_pack" or "time_pass"
3. **Status validation**: Must be "active", "expired", or "suspended"
4. **ValidUntil required for time_pass**: Time passes must have an expiration date
5. **Auto-expiration**: Plans are automatically marked as "expired" when ValidUntil is in the past
6. **MergeAll updates**: Uses Firestore SetOptions.MergeAll to avoid overwriting existing fields

## Features

- **PascalCase Firestore fields**: All Firestore document fields use PascalCase naming
- **camelCase API**: JSON requests/responses use camelCase for external API consistency
- **FirestoreProperty mapping**: Uses `[FirestoreProperty("PascalName")]` for proper field mapping
- **Comprehensive validation**: Business rule validation for all operations
- **Automatic expiration checking**: Expired plans are automatically updated when retrieved
- **Flexible updates**: PATCH endpoint only updates provided fields
- **Credit adjustments**: Dedicated endpoint for credit adjustments with delta values