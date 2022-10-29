#!/bin/sh

RSYNC_OPT='-a --info=name'
PUBLISH_PATH=Publish

rm -rf $PUBLISH_PATH
mkdir -p $PUBLISH_PATH

rsync $RSYNC_OPT LICENSE $PUBLISH_PATH
rsync $RSYNC_OPT About $PUBLISH_PATH
rsync $RSYNC_OPT 1.* $PUBLISH_PATH

rsync $RSYNC_OPT *.sln $PUBLISH_PATH
rsync $RSYNC_OPT Source/*.cs $PUBLISH_PATH/Source
rsync $RSYNC_OPT Source/*.csproj $PUBLISH_PATH/Source
