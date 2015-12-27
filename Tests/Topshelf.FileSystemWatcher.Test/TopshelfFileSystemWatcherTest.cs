using System.IO;
using Moq;
using NUnit.Framework;

namespace Topshelf.FileSystemWatcher.Test
{
    [TestFixture]
    public class TopshelfFileSystemWatcherTest
    {
        private const string _testDir = @"\Test\";
        private const string _testDir2 = @"\Test2\";

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(Directory.GetCurrentDirectory() + _testDir))
                Directory.Delete(Directory.GetCurrentDirectory() + _testDir, true);
            if (Directory.Exists(Directory.GetCurrentDirectory() + _testDir2))
                Directory.Delete(Directory.GetCurrentDirectory() + _testDir2, true);
        }

        [Test, RunInApplicationDomain]
        public void FileSystemChangeEventIsInvokedWithOneDirectoryTest()
        {
            //Arrange
            Mock<IDelegateMock> onChanged = new Mock<IDelegateMock>();

            //Act
            var exitCode = HostFactory.Run(config =>
            {
                config.UseTestHost();

                config.Service<TopshelfFileSystemWatcherTest>(s =>
                {
                    s.ConstructUsing(() => new TopshelfFileSystemWatcherTest());
                    s.WhenStarted((service, host) =>
                    {
                        CreateFile(_testDir + "testFile.Test");
                        return true;
                    });
                    s.WhenStopped((service, host) => true);
                    s.WhenFileSystemCreated(configurator => configurator.AddDirectory(dir =>
                    {
                        dir.Path = Directory.GetCurrentDirectory() + _testDir;
                        dir.CreateDir = true;
                        dir.NotifyFilters = NotifyFilters.FileName;
                    }), onChanged.Object.FileSystemCreated);
                });
            });

            //Assert
            onChanged.Verify(mock => mock.FileSystemCreated(It.IsAny<TopshelfFileSystemEventArgs>()), Times.Once);
            Assert.AreEqual(TopshelfExitCode.Ok, exitCode);
        }

        [Test, RunInApplicationDomain]
        public void FileSystemChangeEventsAreInvokedWithMutipleDirectoriesTest()
        {
            //Arrange
            Mock<IDelegateMock> onChanged = new Mock<IDelegateMock>();

            //Act
            var exitCode = HostFactory.Run(config =>
            {
                config.UseTestHost();

                config.Service<TopshelfFileSystemWatcherTest>(s =>
                {
                    s.ConstructUsing(() => new TopshelfFileSystemWatcherTest());
                    s.WhenStarted((service, host) =>
                    {
                        CreateFile(_testDir + "testFile.Test");
                        CreateFile(_testDir2 + "testFile.Test");
                        return true;
                    });
                    s.WhenStopped((service, host) => true);
                    s.WhenFileSystemCreated(configurator =>
                    {
                        configurator.AddDirectory(dir =>
                        {
                            dir.Path = Directory.GetCurrentDirectory() + _testDir;
                            dir.CreateDir = true;
                            dir.NotifyFilters = NotifyFilters.FileName;
                        });
                        configurator.AddDirectory(dir =>
                        {
                            dir.Path = Directory.GetCurrentDirectory() + _testDir2;
                            dir.CreateDir = true;
                            dir.NotifyFilters = NotifyFilters.FileName;
                        });
                    }, onChanged.Object.FileSystemCreated);
                });
            });

            //Assert
            onChanged.Verify(mock => mock.FileSystemCreated(It.IsAny<TopshelfFileSystemEventArgs>()), Times.Exactly(2));
            Assert.AreEqual(TopshelfExitCode.Ok, exitCode);
        }


        [Test, RunInApplicationDomain]
        public void FileSystemInitialStateEventsAreInvokedCorrectlyTest()
        {
            //Arrange
            Mock<IDelegateMock> onChanged = new Mock<IDelegateMock>();
            CreateFile(_testDir + "initialFile.Test");
            CreateFile(_testDir + "initialFile2.Test");
            CreateFile(_testDir + "initialFile3.Test");

            //Act
            var exitCode = HostFactory.Run(config =>
            {
                config.UseTestHost();

                config.Service<TopshelfFileSystemWatcherTest>(s =>
                {
                    s.ConstructUsing(() => new TopshelfFileSystemWatcherTest());
                    s.WhenStarted((service, host) => true);
                    s.WhenStopped((service, host) => true);
                    s.WhenFileSystemCreated(configurator => configurator.AddDirectory(dir =>
                    {
                        dir.Path = Directory.GetCurrentDirectory() + _testDir;
                        dir.CreateDir = true;
                        dir.GetInitialStateEvent = true;
                    }), onChanged.Object.FileSystemCreated);
                });
            });

            //Assert
            onChanged.Verify(mock => mock.FileSystemCreated(It.IsAny<TopshelfFileSystemEventArgs>()), Times.Exactly(3));
            Assert.AreEqual(TopshelfExitCode.Ok, exitCode);
        }

        [Test, RunInApplicationDomain]
        public void NormaAndInitialFileSystemChangeEventsAreInvokedCorrectlyTest()
        {
            //Arrange
            Mock<IDelegateMock> onChanged = new Mock<IDelegateMock>();
            CreateFile(_testDir + "initialFile.Test");
            CreateFile(_testDir + "initialFile2.Test");
            CreateFile(_testDir + "initialFile3.Test");

            //Act
            var exitCode = HostFactory.Run(config =>
            {
                config.UseTestHost();

                config.Service<TopshelfFileSystemWatcherTest>(s =>
                {
                    s.ConstructUsing(() => new TopshelfFileSystemWatcherTest());
                    s.WhenStarted((service, host) =>
                    {
                        CreateFile(_testDir2 + "testFile.Test");
                        return true;
                    });
                    s.WhenStopped((service, host) => true);
                    s.WhenFileSystemCreated(configurator =>
                    {
                        configurator.AddDirectory(dir =>
                        {
                            dir.Path = Directory.GetCurrentDirectory() + _testDir;
                            dir.CreateDir = true;
                            dir.GetInitialStateEvent = true;
                        });
                        configurator.AddDirectory(dir =>
                        {
                            dir.Path = Directory.GetCurrentDirectory() + _testDir2;
                            dir.CreateDir = true;
                            dir.NotifyFilters = NotifyFilters.FileName;
                            dir.IncludeSubDirectories = true;
                            dir.InternalBufferSize = 8192;
                        });
                    }, onChanged.Object.FileSystemCreated);
                });
            });

            //Assert
            onChanged.Verify(mock => mock.FileSystemCreated(It.IsAny<TopshelfFileSystemEventArgs>()), Times.Exactly(4));
            Assert.AreEqual(TopshelfExitCode.Ok, exitCode);
        }

        [Test, RunInApplicationDomain]
        public void NormalFileSystemChangeEventFilterTest()
        {
            //Arrange
            Mock<IDelegateMock> onChanged = new Mock<IDelegateMock>();

            //Act
            var exitCode = HostFactory.Run(config =>
            {
                config.UseTestHost();

                config.Service<TopshelfFileSystemWatcherTest>(s =>
                {
                    s.ConstructUsing(() => new TopshelfFileSystemWatcherTest());
                    s.WhenStarted((service, host) =>
                    {
                        CreateFile(_testDir + "testFile.Test");
                        return true;
                    });
                    s.WhenStopped((service, host) => true);
                    s.WhenFileSystemCreated(configurator => configurator.AddDirectory(dir =>
                    {
                        dir.Path = Directory.GetCurrentDirectory() + _testDir;
                        dir.CreateDir = true;
                        dir.FileFilter = "*.Test2";
                        dir.NotifyFilters = NotifyFilters.FileName;
                    }), onChanged.Object.FileSystemCreated);
                });
            });

            //Assert
            onChanged.Verify(mock => mock.FileSystemCreated(It.IsAny<TopshelfFileSystemEventArgs>()), Times.Never);
            Assert.AreEqual(TopshelfExitCode.Ok, exitCode);
        }

        [Test, RunInApplicationDomain]
        public void NormalAndInitialFileSystemChangeEventFilterTest()
        {
            //Arrange
            Mock<IDelegateMock> onChanged = new Mock<IDelegateMock>();
            CreateFile(_testDir + "initialFile.Test2");
            CreateFile(_testDir + "initialFile.Test");

            //Act
            var exitCode = HostFactory.Run(config =>
            {
                config.UseTestHost();

                config.Service<TopshelfFileSystemWatcherTest>(s =>
                {
                    s.ConstructUsing(() => new TopshelfFileSystemWatcherTest());
                    s.WhenStarted((service, host) =>
                    {
                        CreateFile(_testDir + "testFile.Test2");
                        CreateFile(_testDir + "testFile.Test");

                        File.Delete(Directory.GetCurrentDirectory() + _testDir + "testFile.Test");
                        File.Delete(Directory.GetCurrentDirectory() + _testDir + "testFile.Test2");

                        return true;
                    });
                    s.WhenStopped((service, host) => true);
                    s.WhenFileSystemChanged(configurator => configurator.AddDirectory(dir =>
                    {
                        dir.Path = Directory.GetCurrentDirectory() + _testDir;
                        dir.CreateDir = true;
                        dir.FileFilter = "*.Test2";
                        dir.NotifyFilters = NotifyFilters.FileName;
                        dir.GetInitialStateEvent = true;
                    }), onChanged.Object.FileSystemChanged, onChanged.Object.FileSystemCreated, onChanged.Object.FileSystemDeleted, onChanged.Object.FileSystemInitial);
                });
            });

            //Assert
            onChanged.Verify(mock => mock.FileSystemChanged(It.IsAny<TopshelfFileSystemEventArgs>()), Times.Exactly(0));
            onChanged.Verify(mock => mock.FileSystemCreated(It.IsAny<TopshelfFileSystemEventArgs>()), Times.Exactly(1));
            onChanged.Verify(mock => mock.FileSystemDeleted(It.IsAny<TopshelfFileSystemEventArgs>()), Times.Exactly(1));
            onChanged.Verify(mock => mock.FileSystemInitial(It.IsAny<TopshelfFileSystemEventArgs>()), Times.Exactly(1));
            Assert.AreEqual(TopshelfExitCode.Ok, exitCode);
        }

        [Test, RunInApplicationDomain]
        public void NormalAndInitialFileSystemChangeEventMultipleFiltersTest()
        {
            //Arrange
            Mock<IDelegateMock> onChanged = new Mock<IDelegateMock>();
            CreateFile(_testDir + "initialFile.Test3");
            CreateFile(_testDir + "initialFile.Test2");
            CreateFile(_testDir + "initialFile1.Test2");
            CreateFile(_testDir + "initialFile.Test");

            //Act
            var exitCode = HostFactory.Run(config =>
            {
                config.UseTestHost();

                config.Service<TopshelfFileSystemWatcherTest>(s =>
                {
                    s.ConstructUsing(() => new TopshelfFileSystemWatcherTest());
                    s.WhenStarted((service, host) =>
                    {
                        CreateFile(_testDir + "testFile.Test3");
                        CreateFile(_testDir + "testFile.Test2");
                        CreateFile(_testDir + "testFile1.Test2");
                        CreateFile(_testDir + "testFile.Test");

                        File.Delete(Directory.GetCurrentDirectory() + _testDir + "testFile.Test");
                        File.Delete(Directory.GetCurrentDirectory() + _testDir + "testFile.Test2");
                        File.Delete(Directory.GetCurrentDirectory() + _testDir + "testFile1.Test2");
                        File.Delete(Directory.GetCurrentDirectory() + _testDir + "testFile.Test3");

                        return true;
                    });
                    s.WhenStopped((service, host) => true);
                    s.WhenFileSystemChanged(configurator =>
                    {
                        configurator.AddDirectory(dir =>
                        {
                            dir.Path = Directory.GetCurrentDirectory() + _testDir;
                            dir.CreateDir = true;
                            dir.FileFilter = "*.Test2";
                            dir.NotifyFilters = NotifyFilters.FileName;
                            dir.GetInitialStateEvent = true;
                        });
                        configurator.AddDirectory(dir =>
                        {
                            dir.Path = Directory.GetCurrentDirectory() + _testDir;
                            dir.CreateDir = true;
                            dir.FileFilter = "*.Test3";
                            dir.NotifyFilters = NotifyFilters.FileName;
                            dir.GetInitialStateEvent = true;
                        });
                    }, onChanged.Object.FileSystemChanged, onChanged.Object.FileSystemCreated,
                        onChanged.Object.FileSystemDeleted, onChanged.Object.FileSystemInitial);
                });
            });

            //Assert
            onChanged.Verify(mock => mock.FileSystemChanged(It.IsAny<TopshelfFileSystemEventArgs>()), Times.Exactly(0));
            onChanged.Verify(mock => mock.FileSystemCreated(It.IsAny<TopshelfFileSystemEventArgs>()), Times.Exactly(3));
            onChanged.Verify(mock => mock.FileSystemDeleted(It.IsAny<TopshelfFileSystemEventArgs>()), Times.Exactly(3));
            onChanged.Verify(mock => mock.FileSystemInitial(It.IsAny<TopshelfFileSystemEventArgs>()), Times.Exactly(3));
            Assert.AreEqual(TopshelfExitCode.Ok, exitCode);
        }

        [Test, RunInApplicationDomain]
        public void NormalAndInitialFileSystemChangeEventMultipleConfiguratorsTest()
        {
            //Arrange
            Mock<IDelegateMock> onChanged = new Mock<IDelegateMock>();
            CreateFile(_testDir + "initialFile.Test3");
            CreateFile(_testDir + "initialFile.Test2");
            CreateFile(_testDir + "initialFile.Test");

            //Act
            var exitCode = HostFactory.Run(config =>
            {
                config.UseTestHost();

                config.Service<TopshelfFileSystemWatcherTest>(s =>
                {
                    s.ConstructUsing(() => new TopshelfFileSystemWatcherTest());
                    s.WhenStarted((service, host) =>
                    {
                        CreateFile(_testDir + "testFile.Test3");
                        CreateFile(_testDir + "testFile.Test2");
                        CreateFile(_testDir + "testFile.Test");

                        File.Delete(Directory.GetCurrentDirectory() + _testDir + "testFile.Test");
                        File.Delete(Directory.GetCurrentDirectory() + _testDir + "testFile.Test2");
                        File.Delete(Directory.GetCurrentDirectory() + _testDir + "testFile.Test3");

                        return true;
                    });
                    s.WhenStopped((service, host) => true);
                    s.WhenFileSystemChanged(configurator =>
                    {
                        configurator.AddDirectory(dir =>
                        {
                            dir.Path = Directory.GetCurrentDirectory() + _testDir;
                            dir.CreateDir = true;
                            dir.FileFilter = "*.Test2";
                            dir.NotifyFilters = NotifyFilters.FileName;
                            dir.GetInitialStateEvent = true;
                        });
                        configurator.AddDirectory(dir =>
                        {
                            dir.Path = Directory.GetCurrentDirectory() + _testDir;
                            dir.CreateDir = true;
                            dir.FileFilter = "*.Test3";
                            dir.NotifyFilters = NotifyFilters.FileName;
                            dir.GetInitialStateEvent = true;
                        });
                        configurator.AddDirectory(dir =>
                        {
                            dir.Path = Directory.GetCurrentDirectory() + _testDir;
                            dir.CreateDir = true;
                            dir.FileFilter = "*.Test";
                            dir.NotifyFilters = NotifyFilters.FileName;
                            dir.GetInitialStateEvent = false;
                        });
                    }, onChanged.Object.FileSystemChanged, onChanged.Object.FileSystemCreated,
                        onChanged.Object.FileSystemDeleted, onChanged.Object.FileSystemInitial);
                });
            });

            //Assert
            onChanged.Verify(mock => mock.FileSystemChanged(It.IsAny<TopshelfFileSystemEventArgs>()), Times.Exactly(0));
            onChanged.Verify(mock => mock.FileSystemCreated(It.IsAny<TopshelfFileSystemEventArgs>()), Times.Exactly(3));
            onChanged.Verify(mock => mock.FileSystemDeleted(It.IsAny<TopshelfFileSystemEventArgs>()), Times.Exactly(3));
            onChanged.Verify(mock => mock.FileSystemInitial(It.IsAny<TopshelfFileSystemEventArgs>()), Times.Exactly(2));
            Assert.AreEqual(TopshelfExitCode.Ok, exitCode);
        }

        private static void CreateFile(string relativePath)
        {
            if (!Directory.Exists(Path.GetFullPath(Directory.GetCurrentDirectory() + relativePath)))
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + Path.GetDirectoryName(relativePath));
            using (FileStream fs = File.Create(Directory.GetCurrentDirectory() + relativePath))
            {
            }
        }
    }

    internal class MyService : ServiceControl
    {
        public bool Start(HostControl hostControl)
        {
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            return true;
        }
    }

    public interface IDelegateMock
    {
        void FileSystemChanged(TopshelfFileSystemEventArgs args);
        void FileSystemCreated(TopshelfFileSystemEventArgs args);
        void FileSystemDeleted(TopshelfFileSystemEventArgs args);
        void FileSystemInitial(TopshelfFileSystemEventArgs args);
    }
}