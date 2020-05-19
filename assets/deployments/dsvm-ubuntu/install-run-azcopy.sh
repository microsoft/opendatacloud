#!/bin/bash
# Installs an runs AzCopy utility
# Logs recorded in /var/lib/waagent/custom-script/download/0/

DATASET_URL=''
DATASET_PATH=''

while true; do
  case "$1" in
    --url ) DATASET_URL="$2"; shift;;
    --path ) DATASET_PATH="$2"; shift;;
    * ) if [ -z "$1" ]; then break; fi;;
  esac
  shift
done

echo "DATASET_URL:  ${DATASET_URL}"
echo "DATASET_PATH: ${DATASET_PATH}"

function showCurrentStatus()
{
    echo '--- Current Status ---'
    systemctl list-units --all apt-daily.service
    ps aux | grep apt
    lsof /var/lib/dpkg/lock
    echo '----------------------'
}

function waitForAptServiceToFinish()
{
    showCurrentStatus
    echo 'Waiting for apt-daily.service to finish ...'
    while [ -n "$(ps aux | grep -F 'apt' | grep -v 'grep')" ]
    do
      sleep 5;
    done
    echo 'Finished waiting for apt-daily.service.'
    sleep 1;
    showCurrentStatus
}

# Determine if azcopy utility is installed
if [ -z "$( which azcopy )" ]
then

    echo 'AzCopy utility not installed.'

    echo 'Adding Microsoft packages source ...'
    curl https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.gpg
    mv microsoft.gpg /etc/apt/trusted.gpg.d/microsoft.gpg
    echo "deb [arch=amd64] https://packages.microsoft.com/repos/microsoft-ubuntu-xenial-prod xenial main" > /etc/apt/sources.list.d/dotnetdev.list

    echo 'Updating packages catalog ...'
    waitForAptServiceToFinish
    apt-get -y update

    echo 'Installing AzCopy ...'
    waitForAptServiceToFinish
    apt-get -y install azcopy

else
    echo 'AzCopy utility already installed.'
fi

# Show versions
echo "version dotnet $(dotnet --info)"
echo "version $(azcopy --version)"

# Copy dataset
echo "Copying dataset ..."
azcopy --source "${DATASET_URL}" --destination ${DATASET_PATH} --recursive --verbose --quiet
echo "Finished copying dataset to ${DATASET_PATH}"
