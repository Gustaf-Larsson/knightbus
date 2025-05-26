##Currently, cosmosBenchmarks must be built using Rider (or manually)

echo "Benchmarks started"
Messages=(10000)
Subscribers=(1)
iterationsPerBenchmark=90

outputFile=delivery_Benchmarksp2.txt
echo "" > "$outputFile" #Create empty file
logFile=delivery_Logs.txt
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
      dotnet run --project CosmosDeliveryBenchmarks.csproj "$msg" >> "$logFile" 2>> "$outputFile"
    done
  done  
done

echo "Benchmarks complete"