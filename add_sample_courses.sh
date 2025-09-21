#!/bin/bash

# Test script to add sample course data to Firestore
# This script assumes the KickRoll API is running on localhost:5112

BASE_URL="http://localhost:5112/api"

echo "Adding sample courses to Firestore..."

# Sample Course 1: 足球基礎班
curl -X POST "${BASE_URL}/courses" \
  -H "Content-Type: application/json" \
  -d '{
    "courseId": "course-001",
    "name": "足球基礎班",
    "description": "適合初學者的足球基礎課程，學習基本技巧和規則",
    "instructorId": "coach-001", 
    "capacity": 20,
    "status": "Active",
    "startDate": "2024-01-15T00:00:00Z",
    "endDate": "2024-06-15T00:00:00Z"
  }'

echo -e "\n"

# Sample Course 2: 進階足球戰術
curl -X POST "${BASE_URL}/courses" \
  -H "Content-Type: application/json" \
  -d '{
    "courseId": "course-002", 
    "name": "進階足球戰術",
    "description": "針對有基礎的學員，學習進階戰術和團隊配合",
    "instructorId": "coach-002",
    "capacity": 15,
    "status": "Active", 
    "startDate": "2024-02-01T00:00:00Z",
    "endDate": "2024-07-01T00:00:00Z"
  }'

echo -e "\n"

# Sample Course 3: 守門員專項訓練
curl -X POST "${BASE_URL}/courses" \
  -H "Content-Type: application/json" \
  -d '{
    "courseId": "course-003",
    "name": "守門員專項訓練", 
    "description": "專門針對守門員位置的技術訓練課程",
    "instructorId": "coach-001",
    "capacity": 8,
    "status": "Active",
    "startDate": "2024-01-20T00:00:00Z", 
    "endDate": "2024-05-20T00:00:00Z"
  }'

echo -e "\n\nSample courses added successfully!"
echo "You can now test the member join course functionality."