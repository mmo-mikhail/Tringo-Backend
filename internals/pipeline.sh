#!/bin/bash

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
WORKSPACE="$(dirname "$DIR")"

function check_error_exit
{
    if [ "$?" = "0" ]; then
	    echo "Stage succeeded!"
    else
        echo "Stage Error!" 1>&2
        exit 1
    fi
}

PROJECTNAME="tringo-api"
IMAGETAG="local"

echo "*****************   building source image"
cd $WORKSPACE
docker build  -t "${PROJECTNAME}":$IMAGETAG .
check_error_exit

echo "*****************   spinning docker service"
docker run -d --rm -p 5000:80 "${PROJECTNAME}":$IMAGETAG
check_error_exit
