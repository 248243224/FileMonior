#include "FileMonitorHelper.h"

class _declspec(dllexport) FileMonitorDll
{
public:
	FileMonitorDll();
	~FileMonitorDll(void);
public:
	static DWORD CALLBACK WorkProc(LPVOID lp);
	static VOID CALLBACK FileIOCompletionRoutine(DWORD dwErrorCode,DWORD dwNumberOfBytesTransfered,LPOVERLAPPED lpOverlapped);
	void SetExcludeDirectory(CHAR* dir);
	HRESULT Start(CHAR* lpFile);  //开启一个线程启动监听
	HRESULT Work();
	HRESULT Stop(); //停止所有线程，以例结束监视
	void ShowFileChange(DWORD dwAction, const FILEDATA& fileData);
	CHAR* OutPut();

private:
	BOOL Exclude(CHAR* dir);
	vector<FILEDATA> m_pfileDataList;
	int m_fileCount;
	CHAR* output;
	bool getoutput;
	DIR_EXCLUDE *exclude;
	FILEDATA FileData;

	//CRITICAL_SECTION m_cs;
};
