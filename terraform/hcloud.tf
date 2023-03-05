# Set the variable value in *.tfvars file
# or using the -var="hcloud_token=..." CLI option
variable "hcloud_token" {
  sensitive = true
}
variable "telegram_bot_token" {
  sensitive = true
}
variable "ssh_private_key" {
  sensitive = true
}
variable "ssh_public_key" {
  sensitive = true
}

# Configure the Hetzner Cloud Provider
provider "hcloud" {
  token = var.hcloud_token
}

resource "hcloud_ssh_key" "ssh_key" {
  name       = "cleannetcode"
  public_key = var.ssh_public_key
}

resource "hcloud_server" "cleannetcode_bot" {
  name        = "cleannetcode-bot"
  image       = "docker-ce"
  server_type = "cx11"
  location    = "nbg1"
  ssh_keys    = [hcloud_ssh_key.ssh_key.id]

  public_net {
    ipv4_enabled = true
    ipv6_enabled = false
  }
}

resource "null_resource" "up_bot_container" {

  triggers = {
    build_number = timestamp()
  }

  connection {
    user        = "root"
    host        = hcloud_server.cleannetcode_bot.ipv4_address
    type        = "ssh"
    private_key = var.ssh_private_key
  }

  provisioner "remote-exec" {
    inline = [
      "docker rm -f $(docker ps -a -q)",
      "docker pull pingvin1308/cleannetcode.bot:0.0.1",
      "docker run -d -e AccessToken=${var.telegram_bot_token} -v /bot_data/Data:/app/Data -v /bot_data/FileStorage:/app/FileStorage pingvin1308/cleannetcode.bot:0.0.1",
      "docker container ls"
    ]
  }
}
