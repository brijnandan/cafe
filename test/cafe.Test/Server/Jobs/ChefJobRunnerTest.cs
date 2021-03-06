using cafe.Server.Jobs;
using cafe.Test.Server.Scheduling;
using FluentAssertions;
using Xunit;

namespace cafe.Test.Server.Jobs
{
    public class ChefJobRunnerTest
    {
        [Fact]
        public void Download_ShouldQueueJob()
        {
            var downloader = new FakeDownloader();
            var job = new DownloadJob(downloader, new FakeClock());
            var runner = JobRunnerTest.CreateJobRunner();
            var jobRunner = new ChefJobRunner(runner, job, CreateInstallChefJob(), RunChefJobTest.CreateRunChefJob());
            const string expectedVersion = "1.2.3";

            jobRunner.DownloadJob.Download(expectedVersion);

            runner.ProcessQueue();

            downloader.DownloadedVersion.Should().Be(expectedVersion, "because the download should have been there");
        }

        private InstallJob CreateInstallChefJob()
        {
            return new InstallJob(new FakeInstaller(), new FakeClock());
        }

        [Fact]
        public void Install_ShouldQueueJob()
        {
            var fakeInstaller = new FakeInstaller();
            var installJob = new InstallJob(fakeInstaller, new FakeClock());
            var runner = JobRunnerTest.CreateJobRunner();
            var jobProcessor = new ChefJobRunner(runner, CreateDownloadJob(), installJob, RunChefJobTest.CreateRunChefJob());

            var expectedVersion = "1.2.3";
            jobProcessor.InstallJob.InstallOrUpgrade(expectedVersion);

            runner.ProcessQueue();

            fakeInstaller.InstalledVersion.ToString().Should().Be(expectedVersion);
        }


        private DownloadJob CreateDownloadJob()
        {
            return new DownloadJob(new FakeDownloader(), new FakeClock());
        }

        [Fact]
        public void LastRun_ShouldDefaultToNullBeforeItRuns()
        {
            var runner = CreateChefJobRunner();
            runner.ToStatus().LastRun.Should().BeNull("because the job has not yet run");
        }

        private ChefJobRunner CreateChefJobRunner()
        {
            return new ChefJobRunner(JobRunnerTest.CreateJobRunner(), CreateDownloadJob(), CreateInstallChefJob(), RunChefJobTest.CreateRunChefJob());
        }
    }
}