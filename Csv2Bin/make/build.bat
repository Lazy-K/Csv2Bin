call _xls2csv.bat -s table1 test.xlsx table1
echo on

rem appendSummary���w�肷��ƃo�C�i���t�@�C���ɗv����(���R�[�h���A���R�[�h�T�C�Y)��ǉ�
..\bin\Debug\Csv2Bin.exe --table table1.csv --manifest $table1.xml --out table1.bin --appendSummary --outcs table1.cs --log log.txt
..\bin\Debug\Csv2Bin.exe --table table1.csv --manifest $table1.xml --out table2.bin

rem cs�t�@�C�������̏o�͂��\
rem ..\bin\Debug\Csv2Bin.exe --manifest $table1.xml --outcs table1.cs

pause
