# Firebase Authentication & Authorization Implementation

This document describes the Firebase Authentication and role-based authorization system implemented for the KickRoll application.

## Overview

The system provides comprehensive user authentication and authorization with three distinct roles:
- **Member**: Basic users who can manage their own data
- **Coach**: Can manage members and class sessions  
- **Admin**: Full system administration capabilities

## Features Implemented

### 1. User Authentication
- Email/password registration with email verification
- Secure login with Firebase custom tokens
- Password reset functionality
- Account status management (Active/Disabled/PendingVerify)

### 2. Role-Based Access Control (RBAC)
- Three roles: Member, Coach, Admin
- Role-based route protection
- Firestore security rules aligned with backend authorization
- Custom claims in Firebase tokens for real-time authorization

### 3. User Management
- Admin panel for user management
- Role assignment and status management
- Search and filtering capabilities
- Comprehensive audit logging

### 4. Security
- JWT-based authentication with Firebase
- Secure API endpoints with role verification
- Firestore security rules preventing unauthorized access
- IP logging for security audit trails

## API Endpoints

### Authentication Endpoints (`/api/auth`)
- `POST /register` - User registration
- `POST /login` - User login
- `POST /verify-email/{userId}` - Email verification
- `POST /send-verification/{userId}` - Resend verification email
- `POST /forgot-password` - Password reset request
- `GET /profile` - Get current user profile
- `POST /setup-admin` - Initial admin setup (environment-protected)
- `POST /logout` - User logout

### User Management Endpoints (`/api/users`) - Admin Only
- `GET /` - List users with filtering
- `PUT /{userId}/role` - Update user role
- `PUT /{userId}/status` - Update user status
- `GET /audit-logs` - View audit logs

## Data Models

### User Document (Firestore: `users/{UserId}`)
```typescript
{
  UserId: string,           // Document ID = Firebase Auth UID
  Name: string,
  Email: string,
  Role: "Member"|"Coach"|"Admin",
  Status: "Active"|"Disabled"|"PendingVerify", 
  EmailVerified: boolean,
  CreatedAt: Timestamp,
  UpdatedAt: Timestamp
}
```

### Audit Log Document (Firestore: `auditLogs/{LogId}`)
```typescript
{
  LogId: string,
  ActorUserId: string,      // Who performed the action
  Action: string,           // Action type (e.g., "ROLE_CHANGED")
  TargetType: string,       // Type of resource (e.g., "User")
  TargetId: string,         // ID of target resource
  Payload?: object,         // Additional action data
  At: Timestamp,
  Ip?: string
}
```

## Role Permissions

### Member
- View own profile and data
- Enroll in/cancel own class registrations
- View available classes and teams
- Manage own member plans

### Coach
- All Member permissions
- View and manage assigned class sessions
- View enrollment lists for their classes
- Cancel enrollments for their classes
- View attendance records
- Manage member plans

### Admin
- All Coach permissions
- Full user management (create, update, disable, role changes)
- View all audit logs
- System configuration access
- Initial admin setup

## Firestore Security Rules

The security rules implement the same permission model:
```javascript
// Core authentication check
function isSignedIn() { return request.auth != null; }
function hasRole(role) { return isSignedIn() && request.auth.token.role == role; }
function isActiveUser() {
  return isSignedIn() && 
         request.auth.token.status == "Active" && 
         request.auth.token.email_verified == true;
}

// Users collection - admins can manage, users can read own
match /users/{userId} {
  allow read: if request.auth.uid == userId || hasRole("Admin");
  allow write: if hasRole("Admin");
}

// Other collections follow similar patterns with role-based access
```

## Initial Setup

### 1. Environment Configuration
Set the following environment variable to allow initial admin creation:
```bash
ALLOW_ADMIN_SETUP=true
```

### 2. Create Initial Admin
```bash
POST /api/auth/setup-admin
{
  "name": "Administrator",
  "email": "admin@example.com", 
  "password": "secure-password"
}
```

### 3. Firestore Rules Deployment
Deploy the `firestore.rules` file to your Firebase project.

## Authentication Flow

1. **Registration**:
   - User registers with email/password
   - Firebase user created with `EmailVerified=false`
   - Firestore user document created with `Role=Member`, `Status=PendingVerify`
   - Email verification sent

2. **Email Verification**:
   - User clicks verification link
   - `EmailVerified=true`, `Status=Active`
   - Custom claims updated

3. **Login**:
   - Email/password validated
   - User status and verification checked
   - Custom token generated with role claims
   - Client receives token for API access

4. **Authorization**:
   - JWT token validated on each API call
   - Role and status checked via custom claims
   - Firestore rules provide additional security layer

## Testing

The implementation includes comprehensive tests:
- Authentication model validation
- Authorization attribute testing
- Role-based access control verification
- Security rule validation scenarios

Run tests with:
```bash
cd KickRoll.Api.Tests
dotnet test
```

## Security Considerations

1. **Email Verification**: Required before account activation
2. **Status Management**: Disabled users cannot access system
3. **Role Synchronization**: Custom claims kept in sync with Firestore
4. **Audit Logging**: All critical actions logged with actor and timestamp
5. **IP Tracking**: Client IP logged for security audits
6. **Token Validation**: Firebase JWT tokens validated on every request

## Future Enhancements

- Multi-factor authentication (MFA)
- OAuth integration (Google, Facebook, etc.)
- Fine-grained permissions beyond basic roles  
- Session management and concurrent login controls
- Password complexity requirements
- Account lockout after failed attempts