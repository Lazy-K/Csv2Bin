call _xls2csv.bat -s table1 test.xlsx table1
echo on

rem appendSummaryを指定するとバイナリファイルに要約情報(レコード数、レコードサイズ)を追加
..\bin\Debug\Csv2Bin.exe --table table1.csv --manifest $table1.xml --out table1.bin --appendSummary --outcs table1.cs --log log.txt
..\bin\Debug\Csv2Bin.exe --table table1.csv --manifest $table1.xml --out table2.bin

rem csファイルだけの出力も可能
rem ..\bin\Debug\Csv2Bin.exe --manifest $table1.xml --outcs table1.cs

pause
