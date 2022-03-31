# Character Controls Demo

This project is a sample character controls demo to demonstrate how you can use Requirements Engineering
to develop a basic character controller.

This project is based off the open source OpenKCC project hosted at
[https://github.com/nicholas-maltbie/OpenKCC](https://github.com/nicholas-maltbie/OpenKCC)

This is an open source project licensed under a [MIT License](LICENSE.txt). Feel free to use a build of the project for
your own work. If you see an error in the project or have any suggestions, write an issue or make a pull request, I'll
happy include any suggestions or ideas into the project.

## Development

If you want to help with the project, feel free to make some changes and submit a PR to the repo.

This project is developed using Unity Release [2021.1.19f1](https://unity3d.com/unity/whats-new/2021.1.19). Install this
version of Unity from Unity Hub using this link:
[unityhub://2021.1.19f1/d0d1bb862f9d](unityhub://2021.1.19f1/d0d1bb862f9d).

### Git LFS Setup

Ensure that you also have git lfs installed. It should be setup to auto-track certain types of files as determined in
the `.gitattributes` file. If the command to install git-lfs `git lfs install` is giving you trouble, try looking into the
[installation guide](https://git-lfs.github.com/).

```PowerShell
# Run this inside the repository after cloning it
# May need to run this on linux
curl -s https://packagecloud.io/install/repositories/github/git-lfs/script.deb.sh | sudo bash
sudo apt-get install git-lfs
```

Once git lfs is installed, from in the repo, run the following command to pull objects for development.

```PowerShell
git lfs pull
```

### Githooks Setup

When working with the project, make sure to setup the `.githooks` if you want to edit the code in the project. In order to
do this, use the following command to reconfigure the `core.hooksPath` for your repository.

```PowerShell
git config --local core.hooksPath .githooks
```
