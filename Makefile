
clean:
	rm -r ./bin/
	rm -r ./obj/
	rm -r ./out/
	rm Release.zip
	rm version.txt

bin/Debug/KSP-AutoLoad.dll:
	msbuild -p:Configuration=Debug

bin/Release/KSP-AutoLoad.dll:
	msbuild -p:Configuration=Release

version.txt: bin/Release/KSP-AutoLoad.dll
	monodis --assembly bin/Release/KSP-AutoLoad.dll | sed -n 's/^Version:\s*\([0-9\.]*\)$$/\1/p' > version.txt

bin/Release/info.json: version.txt
	sed s/version-placeholder/$(shell cat version.txt)/ info.json > bin/Release/info.json

Release.zip: bin/Release/KSP-AutoLoad.dll bin/Release/info.json
	@rm -r out 2>/dev/null || true
	mkdir -p out/GameData/KSP-AutoLoad
	cp bin/Release/KSP-AutoLoad.dll bin/Release/info.json out/GameData/KSP-AutoLoad/
	bsdtar -cvf Release.zip --format=zip -C out GameData


debug: bin/Debug/KSP-AutoLoad.dll
release: Release.zip


