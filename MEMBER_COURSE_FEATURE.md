# Member Join Course Feature Documentation

## Overview
This feature allows members to join courses through the KickRoll application. Members can view available courses in the MemberDetailPage and join them with a single click.

## Architecture

### Backend (API)
- **Course Model** (`KickRoll.Api/Models/Course.cs`): Defines the course structure
- **CoursesController** (`KickRoll.Api/Controllers/CoursesController.cs`): Handles course-related operations

### Frontend (MAUI App)
- **MemberDetailPage** (`KickRoll.App/Views/MemberDetailPage.xaml` & `.xaml.cs`): Shows member details and course management interface
- **Updated navigation** in `MembersListPage.xaml.cs` to use MemberDetailPage

## Firestore Data Structure

### Collections
- `courses/` - Main course collection
- `members/{memberId}/courses/{courseId}` - Member-course enrollment subcollection

### Course Document Structure
```json
{
  "courseId": "course-001",
  "name": "足球基礎班", 
  "description": "適合初學者的足球基礎課程",
  "instructorId": "coach-001",
  "capacity": 20,
  "status": "Active",
  "startDate": "2024-01-15T00:00:00Z",
  "endDate": "2024-06-15T00:00:00Z"
}
```

### Member-Course Enrollment Structure
```json
{
  "CourseId": "course-001",
  "MemberId": "member-123", 
  "JoinedAt": "2024-01-10T10:30:00Z",
  "Status": "Active"
}
```

## API Endpoints

### GET /api/courses/list
Returns all available courses

### GET /api/courses/{courseId}
Gets details for a specific course

### POST /api/courses
Creates a new course

### POST /api/courses/{courseId}/members/{memberId}
Enrolls a member in a course
- **Validation**: Checks if member and course exist
- **Duplicate prevention**: Returns error if member already enrolled

### GET /api/courses/members/{memberId}
Gets all courses a member is enrolled in

### DELETE /api/courses/{courseId}/members/{memberId}
Removes a member from a course

## UI Features

### MemberDetailPage Layout
1. **Member Information** - Basic member details in a gray frame
2. **Enrolled Courses** - Blue frame showing courses the member has joined
3. **Available Courses** - Green frame showing courses available to join
4. **Join buttons** - Individual buttons for each available course
5. **Edit button** - Navigates to EditMemberPage for member information updates

### Key Features
- **Real-time updates**: Course lists refresh after joining/leaving
- **Error handling**: Clear error messages for failed operations
- **Success feedback**: Visual confirmation when courses are joined
- **Loading states**: Buttons show "加入中..." during enrollment

## Validation & Error Handling

### Backend Validation
- Member ID and Course ID cannot be empty
- Member must exist in database
- Course must exist in database  
- Prevents duplicate enrollments

### Frontend Error Handling
- Network error handling with user-friendly messages
- Loading states during API calls
- Success/failure visual feedback

## Testing

### 設置測試資料步驟
1. 確保 Firestore 已正確設置並可連接
2. 啟動 KickRoll API 服務器：`cd KickRoll.Api && dotnet run --urls "http://localhost:5112"`
3. 執行測試資料腳本：`./add_sample_courses.sh`
4. 啟動 MAUI 應用程式測試功能

### 可加入課程的篩選條件
系統會自動篩選符合以下條件的課程：
1. **成員尚未加入**：不在成員的已加入課程清單中
2. **課程狀態為 Active**：`course.Status == "Active"`
3. **課程存在於資料庫**：課程必須存在於 Firestore courses 集合中

### 常見問題排除
- **看不到可加入的課程**：
  - 檢查 API 是否正在運行
  - 確認 Firestore 連接正常
  - 執行 `add_sample_courses.sh` 新增測試資料
  - 確認課程狀態為 "Active"
  - 檢查成員是否已加入所有課程

### Manual Testing Steps
1. Start the API server
2. Run `./add_sample_courses.sh` to add test courses
3. Launch the MAUI app
4. Navigate to Members List
5. Select a member to view MemberDetailPage
6. Try joining available courses
7. Verify courses appear in enrolled section

### Sample Courses
The `add_sample_courses.sh` script creates:
- 足球基礎班 (Football Basics)
- 進階足球戰術 (Advanced Football Tactics) 
- 守門員專項訓練 (Goalkeeper Specialized Training)

## Future Enhancements

### Planned Features
- Course capacity management
- Waitlist functionality
- Course scheduling integration
- Payment integration
- Instructor assignment management
- Course completion tracking

### Technical Improvements
- Offline support with local caching
- Push notifications for course updates
- Bulk enrollment operations
- Advanced filtering and search

## Installation & Setup

1. Ensure Firestore is configured with proper service account keys
2. Build the API project: `dotnet build KickRoll.Api`
3. Build the MAUI app (requires MAUI workloads installed)
4. Run the API server
5. Use the sample data script to populate test courses
6. Launch the MAUI application

## Code Changes Summary

### New Files Added
- `KickRoll.Api/Models/Course.cs` - Course data model
- `KickRoll.Api/Controllers/CoursesController.cs` - Course API endpoints
- `KickRoll.App/Views/MemberDetailPage.xaml` - Member detail UI
- `KickRoll.App/Views/MemberDetailPage.xaml.cs` - Member detail logic
- `add_sample_courses.sh` - Test data script

### Modified Files
- `KickRoll.App/Views/MembersListPage.xaml.cs` - Updated navigation to MemberDetailPage

### Minimal Change Approach
- Reused existing Member model and API infrastructure
- Added new functionality without modifying existing working code
- Maintained existing EditMemberPage for member information editing
- Used established HTTP client patterns from existing pages