provider "aws" {
  region = "ap-south-1" # Mumbai Region
  # AWS credentials local AWS CLI se automatically pick ho jayenge
}

# 1. Security Group (Firewall Rules)
resource "aws_security_group" "support_pulse_sg" {
  name        = "support_pulse_prod_sg"
  description = "Allow HTTP, HTTPS, SSH and App Ports"

  ingress {
    description = "SSH"
    from_port   = 22
    to_port     = 22
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"] # For security, limit this to your IP later
  }

  ingress {
    description = "React Frontend"
    from_port   = 5173
    to_port     = 5173
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  ingress {
    description = "Backend API"
    from_port   = 5215
    to_port     = 5215
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

# 2. EC2 Instance (Production Server)
resource "aws_instance" "app_server" {
  ami           = "ami-03f4878755434977f" # Ubuntu 22.04 LTS (ap-south-1)
  instance_type = "t3.small" # t3.small is better for .NET + AI workloads
  
  security_groups = [aws_security_group.support_pulse_sg.name]

  # Auto-install Docker and Docker Compose on startup
  user_data = <<-EOF
              #!/bin/bash
              apt-get update -y
              apt-get install -y docker.io
              systemctl start docker
              systemctl enable docker
              usermod -aG docker ubuntu
              curl -L "https://github.com/docker/compose/releases/download/v2.24.5/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
              chmod +x /usr/local/bin/docker-compose
              EOF

  tags = {
    Name = "SupportPulse-Prod-Server"
  }
}

# Output the public IP so you can access your app
output "server_public_ip" {
  value = aws_instance.app_server.public_ip
}