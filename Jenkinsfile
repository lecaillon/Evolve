library "pipelineUtils"

pipeline {
    agent { node { label 'docker' } }
    options {
        timestamps()
        disableConcurrentBuilds()
    }
    environment {
        VERSION = pipelineUtils.getSemanticVersion(0, 0)
    }
    stages {
        stage('Build and Publish') {
            steps {
                script {
                    currentBuild.displayName = VERSION
                    pipelineUtils.buildDockerImage()
                    pipelineUtils.runCommandWithDockerImage()

                    if (env.BRANCH_NAME == 'master') {
                        pipelineUtils.createGitTagAndRelease()
                    }
                }
            }
        }
    }
}
