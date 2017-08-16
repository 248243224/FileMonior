#include "stdafx.h"
#include <vector>
#include <stdio.h>

typedef struct ORIGINAL_FILE
{
	WCHAR* oldFile;
	WCHAR* newFile;
	CHAR folderName[1024];
	DWORD action;
}ORIGINAL_FILE;

typedef struct FILE_QUEUE
{
	int size;			//array size
	int head;
	int tail;			//tag of head and tail
	ORIGINAL_FILE* q;	//header pointer
}FILE_QUEUE;

void InitQueue(FILE_QUEUE *Q);
void EnQueue(FILE_QUEUE *Q, ORIGINAL_FILE key);
ORIGINAL_FILE DeQueue(FILE_QUEUE *Q);
BOOL IsQueueEmpty(FILE_QUEUE *Q);
BOOL IsQueueFull(FILE_QUEUE *Q);