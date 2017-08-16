#include "stdafx.h"
#include <iostream> 
//#pragma once
#include "windows.h"
#include <vector>
#include <wchar.h>
#include <tchar.h>  
#include <stdio.h>

using namespace std;
#define MAX_BUF_LEN 100
#define MAX_PATH 1024

typedef struct _FILEDATA
{
	HANDLE hFile;
	CHAR szFolderName[MAX_PATH];
	CHAR szOldFile[MAX_PATH];
	CHAR szNewFile[MAX_PATH];
	HANDLE hEvent;

	HANDLE hThread;
	DWORD dwThreadID;
}FILEDATA, *PFILEDATA;

typedef struct _DIR_EXCLUDE
{
	CHAR dir[MAX_PATH];
	int length;
}DIR_EXCLUDE, *HDIR_EXCLUDE;

typedef struct QUEUE
{
	int size;			//array size
	int head;
	int tail;			//tag of head and tail
	char* q[MAX_PATH];	//header
}QUEUE;

