# RCE (Remote Code Execution Engine)

## Project Description
This project implements a Remote Code Execution (RCE) engine using the following technologies:

Frontend: ReactJS + Vite
Server and Workers: ASP.NET core & Python scripts running in Docker containers on Ubuntu
Message Queue: RabbitMQ
Cache Database: Redis (if needed for persisting data)

## Installation

### Bash
```bash
git clone https://github.com/jeevananthan-23/rce.git
cd rce
Use code with caution.

docker-compose build
Use code with caution.

# Start the Services

docker-compose up -d
```

This framework provides a basic foundation for an RCE engine. You can extend it further by:

Adding support for different programming languages.
Implementing more complex execution environments for specific use cases.
Integrating with existing CI/CD pipelines for automated testing and deployment.
Providing detailed feedback to users regarding code execution results, including error messages and logs.