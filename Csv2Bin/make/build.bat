call _xls2csv.bat -s table1 test.xlsx table1
echo on

rem appendSummary���w�肷��ƃo�C�i���t�@�C���ɗv����(���R�[�h���A���R�[�h�T�C�Y)��ǉ�
..\bin\Debug\Csv2Bin.exe --table table1.csv --manifest table1_csv2bin_manifest.xml --out table1.bin --appendSummary --outcs Cont.cs --log log.txt

rem cs�t�@�C�������̏o�͂��\
rem ..\bin\Debug\Csv2Bin.exe --manifest table1_csv2bin_manifest.xml --outcs Cont.cs

pause
