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
	HRESULT Start(CHAR* lpFile);  //����һ���߳���������
	HRESULT Work();
	HRESULT Stop(); //ֹͣ�����̣߳�������������
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
