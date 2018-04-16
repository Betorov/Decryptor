// This is the main DLL file.

#include "stdafx.h"
#include "RLELib.h"

RLELib::CPackMachine::CPackMachine(String^ str)
{
	m_data = '\0';
	m_count = 0;
	
	IntPtr p = Marshal::StringToHGlobalAnsi(str);
	const char* outFileName = static_cast<const char*>(p.ToPointer());
	pin_ptr<FILE*> outFile = &m_outFile;

	if(str == nullptr)
		throw new std::exception("Output file not write path");

	if (fopen_s(outFile, outFileName, "wb") != 0)
	{
		throw new std::exception("Output file open error");
	}
}

void RLELib::CPackMachine::close()
{
	WriteCollectedData();
	fclose(m_outFile);

	m_outFile = nullptr;
}

RLELib::CPackMachine::~CPackMachine()
{
	if (m_outFile != nullptr) {
		throw new std::exception("Empty outFile");
		return;
	}

	WriteCollectedData();
	fclose(m_outFile);

	m_outFile = nullptr;

}

void RLELib::CPackMachine::SendData(data_t data)
{
	if (m_count == COUNT_MAX || (m_count != 0 && m_data != data))
	{
		WriteCollectedData();
	}

	m_data = data;
	++m_count;
}

void RLELib::CPackMachine::SendFile(String^ str)
{
	FILE *inFile;

	IntPtr p = Marshal::StringToHGlobalAnsi(str);
	const char* inFileName = static_cast<const char*>(p.ToPointer());

	if (str == nullptr)
		throw new std::exception("Input file not path");

	if (fopen_s(&inFile, inFileName, "rb") == 0)
	{
		int tmpChar;
		while ((tmpChar = fgetc(inFile)) != EOF)
		{
			SendData(tmpChar);
		}

		fclose(inFile);
	}
	else
	{
		throw new std::exception("Input file open error");
	}
}

void RLELib::CPackMachine::WriteCollectedData()
{
	/*if (m_outFile != nullptr) {
		throw new std::exception("Empty outFile");
		return;
	}*/

	if (m_count != 0)
	{
		putc(m_count, m_outFile);
		putc(m_data, m_outFile);
	}

	m_count = 0;
}

RLELib::CUnpackMachine::CUnpackMachine(String^ str)
{
	IntPtr p = Marshal::StringToHGlobalAnsi(str);
	const char* outFileName = static_cast<const char*>(p.ToPointer());
	pin_ptr<FILE*> outFile = &m_outFile;

	if (fopen_s(outFile, outFileName, "wb") != 0)
	{
		throw new std::exception("Output file open error");
	}
}

RLELib::CUnpackMachine::~CUnpackMachine()
{
	if (m_outFile != nullptr) {
		fclose(m_outFile);
		m_outFile = nullptr;
	}
}

void RLELib::CUnpackMachine::SendData(count_t count, data_t data)
{
	assert(m_outFile != nullptr);
	assert(count != 0);

	for (count_t i = 0; i < count; ++i)
	{
		putc(data, m_outFile);
	}
}

void RLELib::CUnpackMachine::SendFile(String^ str)
{
	FILE *inFile;

	IntPtr p = Marshal::StringToHGlobalAnsi(str);
	const char* inFileName = static_cast<const char*>(p.ToPointer());

	if (fopen_s(&inFile, inFileName, "rb") != 0)
	{
		throw new std::exception("Input file open error");
	}


	if (fseek(inFile, 0, SEEK_END) != 0)
	{
		fclose(inFile);
		throw new std::exception("Input file open error");
	}

	long int ftellResult = ftell(inFile);

	if (fseek(inFile, 0, SEEK_SET) != 0)
	{
		fclose(inFile);
		throw new std::exception("Input file open error");
	}


	if (ftellResult == -1L)
	{
		fclose(inFile);
		throw new std::exception("Input file open error");
	}

	if (ftellResult % 2 != 0)
	{
		fclose(inFile);
		throw new std::exception("Input file is damaged");
	}


	int countChar, dataChar;
	do
	{
		countChar = fgetc(inFile);
		dataChar = fgetc(inFile);
		assert((countChar == EOF) == (dataChar == EOF));

		if (countChar != EOF)
		{
			if (countChar == 0)
			{
				fclose(inFile);
				throw new std::exception("Input file is damaged");
			}

			SendData(countChar, dataChar);
		}
	} while (countChar != EOF);

	fclose(inFile);
}

void RLELib::CUnpackMachine::close()
{	
		fclose(m_outFile);
		m_outFile = nullptr;
}
