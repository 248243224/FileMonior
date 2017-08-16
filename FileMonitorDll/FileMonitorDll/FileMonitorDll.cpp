// FileMonitorDll.cpp : Defines the exported functions for the DLL application.
#include "stdafx.h"
#include "FileMonitorHelper.h"
#include "FileQueue.h"
#include <comdef.h> 

vector<FILEDATA> m_pfileDataList;
int m_fileCount;
CHAR output[MAX_PATH];
bool excludeinitial = false;
bool queueinitial = false;
DIR_EXCLUDE exclude[MAX_PATH];
CHAR monitortime[MAX_PATH];
FILE_NOTIFY_INFORMATION* pFNI;
FILE_QUEUE queue;
char notify[MAX_PATH];

int getlength(CHAR* str)
{
	int i = 0;
	while (str[i] != '\0')
		i++;
	return i;
}

char* add(char str1[MAX_PATH], char str2[MAX_PATH])
{
	int length1 = getlength(str1);
	int length2 = getlength(str2);
	char *res = (char*)malloc(length1 + length2);
	for (int i = 0; i<length1; i++)
	{
		res[i] = str1[i];
	}
	for (int i = 0; i<length2; i++)
	{
		res[length1 + i] = str2[i];
	}
	res[length1 + length2] = '\0';
	return res;
}

bool comparedir(char *str1, char *str2)
{
	int length1 = getlength(str1);
	int length2 = getlength(str2);
	int j = 0;
	for (int i = 0; i<length1; i++)
	{
		if (str1[i] != str2[j])
			return false;
		//j=0;
		else
			j++;
		if (j == length2)
			return true;
	}
	return false;
}

CHAR* getTime()
{
	SYSTEMTIME sys;
	char a[1024];
	GetLocalTime(&sys);
	sprintf_s(a, "%4d/%02d/%02d %02d:%02d:%02d.%03d",
		sys.wYear,
		sys.wMonth,
		sys.wDay,
		sys.wHour,
		sys.wMinute,
		sys.wSecond,
		sys.wMilliseconds);
	return a;
}

extern "C" __declspec(dllexport) HRESULT Work();
//extern "C" __declspec(dllexport) void ShowFileChange(DWORD dwAction, const FILEDATA &fileData, CHAR time[MAX_PATH]);
extern "C" __declspec(dllexport) BOOL Exclude(CHAR* dir);

DWORD CALLBACK WorkProc(LPVOID lp)
{
	HRESULT hr;
	hr = Work();
	return hr;
}

void CALLBACK FileIOCompletionRoutine(DWORD dwErrorCode, DWORD dwNumberOfBytesTransfered, LPOVERLAPPED lpOverlapped)
{
	//PFILE_NOTIFY_INFORMATION pInfo = pFNI;
	FILEDATA fileData;

	CHAR time[MAX_PATH] = { 0 };
	//strncpy_s(time, getTime(), MAX_PATH);
	//根据句柄查找元素
	for (vector<FILEDATA>::iterator it = m_pfileDataList.begin(); it != m_pfileDataList.end(); ++it)
	{
		if (lpOverlapped->hEvent == it->hEvent)
		{
			fileData = *it;
			break;
		}
	}
	//获取重命名的文件名
	PFILE_NOTIFY_INFORMATION p = (PFILE_NOTIFY_INFORMATION)((char*)pFNI + pFNI->NextEntryOffset);
	
	ORIGINAL_FILE file;
	file.action = pFNI->Action;
	strcpy_s(file.folderName, fileData.szFolderName);
	file.oldFile = pFNI->FileName;
	file.newFile = p->FileName;
	//消息入队
	EnQueue(&queue, file);

	if (dwErrorCode)
		printf("Error Code:%d\r\n", dwErrorCode);
	//printf("Thread ID:%d\r\n",GetCurrentThreadId());  
	SetEvent(lpOverlapped->hEvent);//利用异步过程调用之际，设置事件信号，以便子线程能得到文件改变通知。

}

extern "C" __declspec(dllexport) void SetExcludeDirectory(CHAR* dir)
{
	if (!excludeinitial) {
		//exclude = (DIR_EXCLUDE*)malloc(100);
		exclude->length = 0;
		excludeinitial = true;
		cout << "initial" << endl;
	}
	cout << "Set exclude directory:" << dir << endl;

	int leng = exclude->length;
	strncpy_s(exclude[leng].dir, dir, MAX_PATH); // 最后一个位置为\0保留
	exclude->length++;
}

extern "C" __declspec(dllexport) HRESULT Start(CHAR* lpFile)
{
	DWORD dwThreadID;
	FILEDATA fileData = { 0 };
	TCHAR Dir[MAX_PATH];

	if (!queueinitial)
		InitQueue(&queue);

	//from char to tchar
#	ifdef UNICODE  
	MultiByteToWideChar(CP_ACP, 0, lpFile, -1, Dir, 100);
#	else  
	strcpy(Name, strUsr);
#	endif

	memset(fileData.szFolderName, 0, MAX_PATH);
	strcpy_s(fileData.szFolderName, lpFile);

	fileData.hFile = CreateFile(Dir,
		FILE_LIST_DIRECTORY,  //表明打开一个目录
		FILE_SHARE_DELETE | FILE_SHARE_READ | FILE_SHARE_WRITE,
		NULL,
		OPEN_EXISTING,
		FILE_FLAG_BACKUP_SEMANTICS | FILE_FLAG_OVERLAPPED,//FILE_FLAG_OVERLAPPED表示异步模式
		NULL);

	if (fileData.hFile == INVALID_HANDLE_VALUE)
	{
		printf("open directory failed\r\n");
		system("pause");
		return -1;
	}

	cout << "Monitor Start:" << lpFile << endl;

	pFNI = (FILE_NOTIFY_INFORMATION*)notify;
	memset(notify, 0, strlen(notify));

	fileData.hEvent = CreateEvent(NULL, TRUE, FALSE, NULL);
	fileData.hThread = CreateThread(NULL, 0, WorkProc, Start, CREATE_SUSPENDED, &dwThreadID);
	fileData.dwThreadID = dwThreadID;
	m_pfileDataList.push_back(fileData);
	ResumeThread(fileData.hThread);
	return S_OK;
}


extern "C" __declspec(dllexport) HRESULT Work()
{
	FILEDATA fileData;
	DWORD dwByteReturn;
	DWORD dwCurrentThreadID;
	OVERLAPPED over = { 0 };
	dwCurrentThreadID = GetCurrentThreadId();

	DWORD dwBufLen = 2 * (sizeof(FILE_NOTIFY_INFORMATION) + MAX_PATH * sizeof(TCHAR));
	FILE_NOTIFY_INFORMATION* m_pFNI = (FILE_NOTIFY_INFORMATION*)HeapAlloc(GetProcessHeap(), HEAP_ZERO_MEMORY, dwBufLen);

	//根据当前线程ID查找元素
	for (vector<FILEDATA>::iterator it = m_pfileDataList.begin(); it != m_pfileDataList.end(); ++it)
	{
		if (dwCurrentThreadID == it->dwThreadID)
		{
			fileData = *it;
			break;
		}
	}

	while (true)
	{
		over.hEvent = fileData.hEvent;

		//if(ReadDirectoryChangesW(fileData.hFile,m_pFNI, MAX_PATH,TRUE,
		//FILE_NOTIFY_CHANGE_FILE_NAME		//要求监视文件名称的改变
		//|FILE_NOTIFY_CHANGE_DIR_NAME		//要求监视目录名称的改变
		//| FILE_NOTIFY_CHANGE_SIZE,			//求监视文件大小的改变
		//      //| FILE_NOTIFY_CHANGE_LAST_WRITE,	//要求监视最后写入时间的改变
		//&dwByteReturn, &over,
		//(LPOVERLAPPED_COMPLETION_ROUTINE)FileIOCompletionRoutine))

		if (ReadDirectoryChangesW(fileData.hFile, pFNI, MAX_PATH, TRUE,
			FILE_NOTIFY_CHANGE_FILE_NAME		//要求监视文件名称的改变
			| FILE_NOTIFY_CHANGE_DIR_NAME		//要求监视目录名称的改变
			| FILE_NOTIFY_CHANGE_SIZE,			//求监视文件大小的改变
			&dwByteReturn, &over,
			(LPOVERLAPPED_COMPLETION_ROUTINE)FileIOCompletionRoutine))
		{
			DWORD dwResult;
			dwResult = WaitForSingleObjectEx(fileData.hEvent, INFINITE, TRUE); //等待异步过程被调用与事件变成信号态   
																			   //strncpy_s(monitortime, getTime(), MAX_PATH);
			ResetEvent(fileData.hEvent);

			//if (dwResult == WAIT_OBJECT_0)
			//{
			//	//获取重命名的文件名
			//	PFILE_NOTIFY_INFORMATION p = (PFILE_NOTIFY_INFORMATION)((char*)m_pFNI + m_pFNI->NextEntryOffset);

			//	ORIGINAL_FILE file;
			//	file.action = m_pFNI->Action;
			//	strcpy_s(file.folderName, fileData.szFolderName);
			//	file.oldFile = m_pFNI->FileName;
			//	file.newFile = p->FileName;
			//	//消息入队
			//	EnQueue(&queue, file);

			//	//CHAR time[MAX_PATH]={0};
			//	//strncpy_s(time, getTime(), MAX_PATH);

			//	////转换文件名为多字节字符串
			//	//if (m_pFNI->FileName)
			//	//{
			//	//	memset(fileData.szOldFile, 0, strlen(fileData.szOldFile));
			//	//	WideCharToMultiByte(CP_ACP, 0, m_pFNI->FileName, m_pFNI->FileNameLength / 2, fileData.szOldFile, 99, NULL, NULL);
			//	//}

			//	////获取重命名的文件名
			//	//if (m_pFNI->NextEntryOffset != 0 && (m_pFNI->FileNameLength > 0 && m_pFNI->FileNameLength < MAX_PATH))
			//	//{
			//	//	PFILE_NOTIFY_INFORMATION p = (PFILE_NOTIFY_INFORMATION)((char*)m_pFNI + m_pFNI->NextEntryOffset);
			//	//	memset(fileData.szNewFile, 0, sizeof(fileData.szNewFile));
			//	//	WideCharToMultiByte(CP_ACP, 0, p->FileName, p->FileNameLength / 2, fileData.szNewFile, 99, NULL, NULL);
			//	//}
			//
			//	//CHAR filepath[MAX_PATH];
			//	//sprintf_s(filepath, "%s\\%s", fileData.szFolderName, fileData.szOldFile);
			//	////是否被屏蔽文件夹
			//	//if (!Exclude(filepath))
			//	//	ShowFileChange(m_pFNI->Action, fileData,monitortime);
			//	ResetEvent(fileData.hEvent);
			//}
			//else if(dwResult==WAIT_IO_COMPLETION)
			//{
			//	printf("wating async using...\r\n");
			//}

		}
		else
		{
			DWORD err = GetLastError();
			if (GetLastError() == ERROR_INVALID_PARAMETER)
				printf("缓冲区太大\r\n");
			break;
		}
	}
	HeapFree(GetProcessHeap(), 0, m_pFNI);
	DWORD dwErr = GetLastError();
	if (ERROR_INVALID_HANDLE != dwErr)
	{
		return HRESULT_FROM_WIN32(dwErr);
	}
	return S_OK;
}


extern "C" __declspec(dllexport) BOOL Exclude(CHAR* dir)
{
	for (int i = 0; i<exclude->length; i++)
	{
		char* a = exclude[i].dir;
		if (comparedir(dir, a))
			return true;
	}
	return false;
}

extern "C" __declspec(dllexport) CHAR* __stdcall OutPut()
{
Listen:
	while (IsQueueEmpty(&queue)) {}
	ORIGINAL_FILE file = DeQueue(&queue);
	char OldFile[MAX_PATH];
	char NewFile[MAX_PATH];
	char Folder[MAX_PATH];

	_bstr_t oldFile(file.oldFile);
	_bstr_t newFile(file.newFile);
	strcpy_s(OldFile, oldFile);
	strcpy_s(NewFile, newFile);
	strcpy_s(Folder, file.folderName);

	//是否屏蔽文件夹
	CHAR filepath[MAX_PATH];
	sprintf_s(filepath, "%s\\%s", Folder, OldFile);
	if (Exclude(filepath))
		goto Listen;

	switch (file.action)
	{
	case FILE_ACTION_ADDED:
		sprintf_s(output, "file added: %s\\%s", Folder, OldFile);
		break;
	case FILE_ACTION_MODIFIED:
		sprintf_s(output, "file modified: %s\\%s", Folder, OldFile);
		break;
	case FILE_ACTION_REMOVED:
		sprintf_s(output, "file removed: %s\\%s", Folder, OldFile);
		break;
	case FILE_ACTION_RENAMED_OLD_NAME:
		sprintf_s(output, "file renamed: %s\\%s to: %s\\%s", Folder, OldFile, Folder, NewFile);
		break;
	default:
		sprintf_s(output, "unknown command!");
	}
	return output;
}

extern "C" __declspec(dllexport) HRESULT Stop()
{
	HANDLE* hThreadList = (HANDLE*)LocalAlloc(LPTR, sizeof(HANDLE)*m_pfileDataList.size());
	int i = 0;
	for (vector<FILEDATA>::iterator it = m_pfileDataList.begin(); it != m_pfileDataList.end(); ++it)
	{
		CloseHandle(it->hFile);
		hThreadList[i++] = it->hThread;
	}
	WaitForMultipleObjects(i, hThreadList, TRUE, INFINITE);
	for (vector<FILEDATA>::iterator it = m_pfileDataList.begin(); it != m_pfileDataList.end(); ++it)
	{
		CloseHandle(it->hThread);
		CloseHandle(it->hEvent);
	}
	LocalFree(hThreadList);
	hThreadList = NULL;

	m_pfileDataList.clear();
	m_fileCount = 0;
	return S_OK;
}

extern "C" __declspec(dllexport) CHAR* __stdcall test(int a)
{
	if (a == 1)
		return "this is 1";
	return "this is not 1";
}