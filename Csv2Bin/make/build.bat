call _xls2csv.bat -s table1 test.xlsx table1

..\bin\Debug\Csv2Bin.exe --table table1.csv --manifest $table1.xml --out table1.bin --appendSummary --outcs table1.cs
..\bin\Debug\Csv2Bin.exe --table table1.csv --manifest $table1.xml --out table2.bin


pause
