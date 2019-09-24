call _xls2csv.bat -s table1 test.xlsx table1
call _xls2csv.bat -s $table1 test.xlsx $table1

..\bin\Debug\Csv2Bin.exe --table table1.csv --manifest $table1.csv --out table1.bin --appendSummary
..\bin\Debug\Csv2Bin.exe --table table1.csv --manifest $table1.csv --out table2.bin

