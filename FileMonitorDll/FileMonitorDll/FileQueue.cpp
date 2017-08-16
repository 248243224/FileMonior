#include "stdafx.h"
#include "FileQueue.h"

void InitQueue(FILE_QUEUE *Q)
{
	Q->size = 512;

	Q->q = (ORIGINAL_FILE *)malloc(sizeof(ORIGINAL_FILE) * Q->size); //�����ڴ�
																	 //Q->q = (char(*)[MAX_PATH])malloc(10); //�����ڴ�
	Q->tail = 0;
	Q->head = 0;
}

void EnQueue(FILE_QUEUE *Q, ORIGINAL_FILE key)
{
	int tail = (Q->tail + 1) % Q->size; //ȡ�ౣ֤����quil=queuesize-1ʱ����ת��0
	if (tail == Q->head)              //��ʱ����û�пռ�
	{
		printf("the queue has been filled full!");
	}
	else
	{
		Q->q[Q->tail] = key;
		Q->tail = tail;
	}
}

ORIGINAL_FILE DeQueue(FILE_QUEUE *Q)
{
	ORIGINAL_FILE tmp;
	if (Q->tail == Q->head)     //�ж϶��в�Ϊ��
	{
		printf("the queue is NULL\n");
	}
	else
	{
		tmp = Q->q[Q->head];
		Q->head = (Q->head + 1) % Q->size;
	}
	return tmp;
}

BOOL IsQueueEmpty(FILE_QUEUE *Q)
{
	if (Q->head == Q->tail)
		return true;
	else
		return false;
}

BOOL IsQueueFull(FILE_QUEUE *Q)
{
	if ((Q->tail + 1) % Q->size == Q->head)
		return true;
	else
		return false;
}
