// RLELib.h

#pragma once

using namespace System;

namespace RLELib {

	public ref class CPackMachine
	{
	public:
		typedef uint8_t count_t;
		static const count_t COUNT_MAX = UINT8_MAX;
		typedef char data_t;

		CPackMachine(String^ str);
		void close();
		~CPackMachine();
		void SendData(data_t data);
		void SendFile(String^ str);

	protected:
		data_t m_data;
		count_t m_count;
		FILE* m_outFile;


		void WriteCollectedData();
	};

	public ref class CUnpackMachine
	{
	public:
		typedef uint8_t count_t;
		typedef char data_t;

		CUnpackMachine(String^ str);
		~CUnpackMachine();
		void SendData(count_t count, data_t data);
		void SendFile(String^ str);
		void close();
	protected:
		FILE *m_outFile;
	};
}
