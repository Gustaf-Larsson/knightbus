##Currently, cosmosBenchmarks must be built using Rider (or manually)

echo "Benchmarks started"
Messages=(1000 10000 50000 100000)
Subscribers=(1)
iterationsPerBenchmark=10

outputFile=latency_Benchmarks.txt
echo "" > "$outputFile" #Create empty file
logFile=LogsLarge.txt
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
      dotnet run --project CosmosLatencyBenchmarks.csproj "$msg" "$subs" "output" >> "$logFile" 2>> "$outputFile"
    done
  done  
done

echo "Benchmarks complete"