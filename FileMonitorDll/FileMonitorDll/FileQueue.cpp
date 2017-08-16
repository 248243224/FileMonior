#include "stdafx.h"
#include "FileQueue.h"

void InitQueue(FILE_QUEUE *Q)
{
	Q->size = 512;

	Q->q = (ORIGINAL_FILE *)malloc(sizeof(ORIGINAL_FILE) * Q->size); //分配内存
																	 //Q->q = (char(*)[MAX_PATH])malloc(10); //分配内存
	Q->tail = 0;
	Q->head = 0;
}

void EnQueue(FILE_QUEUE *Q, ORIGINAL_FILE key)
{
	int tail = (Q->tail + 1) % Q->size; //取余保证，当quil=queuesize-1时，再转回0
	if (tail == Q->head)              //此时队列没有空间
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
	if (Q->tail == Q->head)     //判断队列不为空
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
