#!/bin/bash

for config in `ls *.json`;
do
	cd ..
	echo $config
	targetdir="Builds/${config%.json}"
	mkdir $targetdir
	cp -r Build/* $targetdir
	cp configs/$config $targetdir/config.json
	cd -
done


cd ../Builds

for i in {1..4}
do
	echo $i
	zip -q "participant_${i}.zip" -r "remote_${i}_arvsvr" "remote_${i}_stream"
	echo $i BACKUP
	zip -q "participant_${i}_BACKUP.zip" -r "remote_${i}_arvsvr_BACKUP" "remote_${i}_stream_BACKUP"
done

cd -
