.PHONY: all

all: build

build:
	dotnet build

check: build
	dotnet test tests/fwrules.tests.fsproj
