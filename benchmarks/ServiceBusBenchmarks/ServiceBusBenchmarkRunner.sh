##Currently, cosmosBenchmarks must be built using Rider (or manually)

echo "ServiceBus Benchmarks started"
Messages=(100000)
Subscribers=(2 8)
iterationsPerBenchmark=5

outputFile=huge28Benchmarks.txt
echo "" > "$outputFile" #Create empty file
logFile=huge28Logs.txt
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
      dotnet run --project ServiceBusBenchmarks.csproj "$msg" "$subs" "output" >> "$logFile" 2>> "$outputFile"
    done
  done  
done

echo "Benchmarks complete"