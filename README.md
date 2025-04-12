# imperium
Automation and control software.

## Run in docker

### build
docker build -t imperium-ssh .

### run
docker run -d -p 2222:22 -p 5000:5000 --name imperium-container imperium-ssh


