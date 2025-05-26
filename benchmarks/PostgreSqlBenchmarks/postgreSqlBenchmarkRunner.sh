##Currently, cosmosBenchmarks must be built using Rider (or manually)

echo "PostgreSql Benchmarks started"
Messages=(1000 10000 50000 100000)
Subscribers=(1 8 16)
iterationsPerBenchmark=1

outputFile=iopsBenchmarks.txt
echo "" > "$outputFile" #Create empty file
logFile=iopssmallLogs.txt
echo "" > "$logFile" #Create empty log file

for msg in "${Messages[@]}";
  do
  echo ............ Messages: "$msg" >> "$outputFile"
  for subs in "${Subscribers[@]}";
    do
    echo ---------- Subscribers: "$subs" >> "$outputFile"     
    for ((i=1; i<=iterationsPerBenchmark; i++))
    do
      echo "Messages: $msg | Subs: $subs | Iteration: $i"
      dotnet run --project PostgreSqlBenchmarks.csproj "$msg" "$subs" "output" >> "$logFile" 2>> "$outputFile"
    done
  done  
done

echo "Benchmarks complete"