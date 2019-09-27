call _xls2csv.bat -s table1 test.xlsx table1
echo on

rem appendSummaryを指定するとバイナリファイルに要約情報(レコード数、レコードサイズ)を追加
..\bin\Debug\Csv2Bin.exe --table table1.csv --manifest table1_csv2bin_manifest.xml --out table1.bin --appendSummary --outcs Cont.cs --log log.txt

rem csファイルだけの出力も可能
rem ..\bin\Debug\Csv2Bin.exe --manifest table1_csv2bin_manifest.xml --outcs Cont.cs

pause
