# Installing Podman to run Dockerfile locally
* Install WSL2
    * Open `cmd` as Administrator
    * Run `wsl --install`
    * Restart Windows
* Install a Linux Distro
    * Open `cmd` as Administrator
    * Run `wsl --install -d Ubuntu` to install Ubuntu LTS
    * Restart Windows
* Add a Repositories which has Podman available
    * If you are using Ubuntu 20.10 or newer Podman is available in the official repositories. And skip to the next part of adding a repository to find Podman.
    * If you are running 20.04 (current LTS), or newer run the below.
    * Open an Ubuntu terminal window, and run the below.
        ```
        . /etc/os-release
        echo "deb https://download.opensuse.org/repositories/devel:/kubic:/libcontainers:/stable/xUbuntu_${VERSION_ID}/ /" | sudo tee /etc/apt/sources.list.d/devel:kubic:libcontainers:stable.list
        curl -L "https://download.opensuse.org/repositories/devel:/kubic:/libcontainers:/stable/xUbuntu_${VERSION_ID}/Release.key" | sudo apt-key add -
        ```
* Install Podman
    * In an Ubuntu terminal, run the below
        ```
        sudo apt-get update
        sudo apt-get -y upgrade
        sudo apt-get -y install podman
        ```