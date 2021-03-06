using Jering.IocServices.System.IO;
using Jering.Markdig.Extensions.FlexiBlocks.FlexiIncludeBlocks;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Jering.Markdig.Extensions.FlexiBlocks.Tests.FlexiIncludeBlocks
{
    public class DiskCacheServiceUnitTests : IClassFixture<DiskCacheServiceUnitTestsFixture>
    {
        private readonly string _dummyFile;
        private readonly MockRepository _mockRepository = new MockRepository(MockBehavior.Default) { DefaultValue = DefaultValue.Mock };

        public DiskCacheServiceUnitTests(DiskCacheServiceUnitTestsFixture fixture)
        {
            // No easy way to create a dummy FileStream, so we have to create a dummy file
            _dummyFile = Path.Combine(fixture.TempDirectory, "dummyFile");
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullExceptionIfFileServiceIsNull()
        {
            // Act and assert
            Assert.Throws<ArgumentNullException>(() => new DiskCacheService(
                null,
                _mockRepository.Create<IDirectoryService>().Object,
                _mockRepository.Create<ILoggerFactory>().Object));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullExceptionIfDirectoryServiceIsNull()
        {
            // Act and assert
            Assert.Throws<ArgumentNullException>(() => new DiskCacheService(
                _mockRepository.Create<IFileService>().Object,
                null,
                _mockRepository.Create<ILoggerFactory>().Object));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullExceptionIfLoggerFactoryIsNull()
        {
            // Act and assert
            Assert.Throws<ArgumentNullException>(() => new DiskCacheService(
                _mockRepository.Create<IFileService>().Object,
                _mockRepository.Create<IDirectoryService>().Object,
                null));
        }

        [Theory]
        [MemberData(nameof(TryGetCacheFile_ThrowsArgumentExceptionIfIdentifierIsNullWhiteSpaceOrAnEmptyString_Data))]
        public void TryGetCacheFile_ThrowsArgumentExceptionIfIdentifierIsNullWhiteSpaceOrAnEmptyString(string dummySource)
        {
            // Arrange
            DiskCacheService testSubject = CreateDiskCacheService();

            // Act and assert
            ArgumentException result = Assert.Throws<ArgumentException>(() => testSubject.TryGetCacheFile(dummySource, null));
            Assert.Equal(string.Format(Strings.ArgumentException_Shared_ValueCannotBeNullWhitespaceOrAnEmptyString, "identifier"), result.Message);
        }

        public static IEnumerable<object[]> TryGetCacheFile_ThrowsArgumentExceptionIfIdentifierIsNullWhiteSpaceOrAnEmptyString_Data()
        {
            return new object[][]
            {
                new object[]{null},
                new object[]{""},
                new object[]{" "}
            };
        }

        [Theory]
        [MemberData(nameof(TryGetCacheFile_ThrowsArgumentExceptionIfCacheDirectoryIsNullWhiteSpaceOrAnEmptyString_Data))]
        public void TryGetCacheFile_ThrowsArgumentExceptionIfCacheDirectoryIsNullWhiteSpaceOrAnEmptyString(string dummyCacheDirectory)
        {
            // Arrange
            const string dummySource = "dummySource";
            DiskCacheService testSubject = CreateDiskCacheService();

            // Act and assert
            ArgumentException result = Assert.Throws<ArgumentException>(() => testSubject.TryGetCacheFile(dummySource, dummyCacheDirectory));
            Assert.Equal(string.Format(Strings.ArgumentException_Shared_ValueCannotBeNullWhitespaceOrAnEmptyString, "cacheDirectory"), result.Message);
        }

        public static IEnumerable<object[]> TryGetCacheFile_ThrowsArgumentExceptionIfCacheDirectoryIsNullWhiteSpaceOrAnEmptyString_Data()
        {
            return new object[][]
            {
                new object[]{null},
                new object[]{""},
                new object[]{" "}
            };
        }

        [Fact]
        public void TryGetCacheFile_ReturnsNullIfCacheFileDoesNotExist()
        {
            // Arrange
            const string dummySource = "dummySource";
            const string dummyCacheDirectory = "dummyCacheDirectory";
            const string dummyFilePath = "dummyFilePath";
            Mock<IFileService> mockFileService = _mockRepository.Create<IFileService>();
            mockFileService.Setup(f => f.Exists(dummyFilePath)).Returns(false);
            Mock<DiskCacheService> mockTestSubject = CreateMockDiskCacheService(fileService: mockFileService.Object);
            mockTestSubject.CallBase = true;
            mockTestSubject.Setup(t => t.CreatePath(dummySource, dummyCacheDirectory)).Returns(dummyFilePath);

            // Act
            FileStream resultFileStream = mockTestSubject.Object.TryGetCacheFile(dummySource, dummyCacheDirectory);

            // Assert
            _mockRepository.VerifyAll();
            Assert.Null(resultFileStream);
        }

        [Fact]
        public void TryGetCacheFile_ReturnsAFileStreamIfCacheFileExistsAndCanBeOpened()
        {
            // Arrange
            const string dummySource = "dummySource";
            const string dummyCacheDirectory = "dummyCacheDirectory";
            const string dummyFilePath = "dummyFilePath";
            using (FileStream dummyFileStream = File.Open(_dummyFile, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite))
            {
                Mock<IFileService> mockFileService = _mockRepository.Create<IFileService>();
                mockFileService.Setup(f => f.Exists(dummyFilePath)).Returns(true);
                Mock<DiskCacheService> mockTestSubject = CreateMockDiskCacheService(fileService: mockFileService.Object);
                mockTestSubject.CallBase = true;
                mockTestSubject.Setup(t => t.CreatePath(dummySource, dummyCacheDirectory)).Returns(dummyFilePath);
                mockTestSubject.Setup(t => t.GetStream(dummyFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)).Returns(dummyFileStream);

                // Act
                FileStream resultFileStream = mockTestSubject.Object.TryGetCacheFile(dummySource, dummyCacheDirectory);

                // Assert
                _mockRepository.VerifyAll();
                Assert.Same(dummyFileStream, resultFileStream);
            }
        }

        [Theory]
        [MemberData(nameof(TryGetCacheFile_ReturnsNullIfCacheFileIsDeletedBetweenFileExistsAndFileOpen_Data))]
        public void TryGetCacheFile_ReturnsNullIfCacheFileIsDeletedBetweenFileExistsAndFileOpen(ISerializableWrapper<Exception> dummyExceptionWrapper)
        {
            // Arrange
            const string dummyCacheDirectory = "dummyCacheDirectory";
            const string dummySource = "dummySource";
            const string dummyFilePath = "dummyFilePath";
            Mock<IFileService> mockFileService = _mockRepository.Create<IFileService>();
            mockFileService.Setup(f => f.Exists(dummyFilePath)).Returns(true);
            Mock<DiskCacheService> mockTestSubject = CreateMockDiskCacheService(fileService: mockFileService.Object);
            mockTestSubject.CallBase = true;
            mockTestSubject.Setup(t => t.CreatePath(dummySource, dummyCacheDirectory)).Returns(dummyFilePath);
            mockTestSubject.Setup(t => t.GetStream(dummyFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)).Throws(dummyExceptionWrapper.Value);

            // Act
            FileStream resultFileStream = mockTestSubject.Object.TryGetCacheFile(dummySource, dummyCacheDirectory);

            // Assert
            _mockRepository.VerifyAll();
            Assert.Null(resultFileStream);
        }

        public static IEnumerable<object[]> TryGetCacheFile_ReturnsNullIfCacheFileIsDeletedBetweenFileExistsAndFileOpen_Data()
        {
            return new object[][]
            {
                new object[]{new SerializableWrapper<FileNotFoundException>(new FileNotFoundException())},
                new object[]{new SerializableWrapper<DirectoryNotFoundException>(new DirectoryNotFoundException())},
            };
        }

        [Fact]
        public void TryGetCacheFile_ThrowsInvalidOperationExceptionIfAnUnexpectedExceptionIsThrownWhenAttemptingToOpenAStream()
        {
            // Arrange
            const string dummyCacheDirectory = "dummyCacheDirectory";
            const string dummySource = "dummySource";
            const string dummyFilePath = "dummyFilePath";
            Mock<IFileService> mockFileService = _mockRepository.Create<IFileService>();
            mockFileService.Setup(f => f.Exists(dummyFilePath)).Returns(true);
            var dummyException = new IOException();
            Mock<DiskCacheService> mockTestSubject = CreateMockDiskCacheService(fileService: mockFileService.Object);
            mockTestSubject.CallBase = true;
            mockTestSubject.Setup(t => t.CreatePath(dummySource, dummyCacheDirectory)).Returns(dummyFilePath);
            mockTestSubject.Setup(t => t.GetStream(dummyFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)).Throws(dummyException);

            // Act and assert
            InvalidOperationException result = Assert.Throws<InvalidOperationException>(() => mockTestSubject.Object.TryGetCacheFile(dummySource, dummyCacheDirectory));
            _mockRepository.VerifyAll();
            Assert.Equal(string.Format(Strings.InvalidOperationException_DiskCacheService_UnexpectedDiskCacheException, dummySource, dummyFilePath), result.Message);
            Assert.Same(dummyException, result.InnerException);
        }

        [Theory]
        [MemberData(nameof(CreateOrGetCacheFile_ThrowsArgumentExceptionIfIdentifierIsNullWhiteSpaceOrAnEmptyString_Data))]
        public void CreateOrGetCacheFile_ThrowsArgumentExceptionIfIdentifierIsNullWhiteSpaceOrAnEmptyString(string dummySource)
        {
            // Arrange
            DiskCacheService testSubject = CreateDiskCacheService();

            // Act and assert
            ArgumentException result = Assert.Throws<ArgumentException>(() => testSubject.CreateOrGetCacheFile(dummySource, null));
            Assert.Equal(string.Format(Strings.ArgumentException_Shared_ValueCannotBeNullWhitespaceOrAnEmptyString, "identifier"), result.Message);
        }

        public static IEnumerable<object[]> CreateOrGetCacheFile_ThrowsArgumentExceptionIfIdentifierIsNullWhiteSpaceOrAnEmptyString_Data()
        {
            return new object[][]
            {
                new object[]{null},
                new object[]{""},
                new object[]{" "}
            };
        }

        [Theory]
        [MemberData(nameof(CreateOrGetCacheFile_ThrowsArgumentExceptionIfCacheDirectoryIsNullWhiteSpaceOrAnEmptyString_Data))]
        public void CreateOrGetCacheFile_ThrowsArgumentExceptionIfCacheDirectoryIsNullWhiteSpaceOrAnEmptyString(string dummyCacheDirectory)
        {
            // Arrange
            const string dummySource = "dummySource";
            DiskCacheService testSubject = CreateDiskCacheService();

            // Act and assert
            ArgumentException result = Assert.Throws<ArgumentException>(() => testSubject.CreateOrGetCacheFile(dummySource, dummyCacheDirectory));
            Assert.Equal(string.Format(Strings.ArgumentException_Shared_ValueCannotBeNullWhitespaceOrAnEmptyString, "cacheDirectory"), result.Message);
        }

        public static IEnumerable<object[]> CreateOrGetCacheFile_ThrowsArgumentExceptionIfCacheDirectoryIsNullWhiteSpaceOrAnEmptyString_Data()
        {
            return new object[][]
            {
                new object[]{null},
                new object[]{""},
                new object[]{" "}
            };
        }

        [Fact]
        public void CreateOrGetCacheFile_ThrowsInvalidOperationExceptionIfCacheDirectoryIsInvalid()
        {
            // Arrange
            const string dummySource = "dummySource";
            const string dummyCacheDirectory = "dummyCacheDirectory";
            var dummyException = new IOException();
            Mock<IDirectoryService> mockDirectoryService = _mockRepository.Create<IDirectoryService>();
            mockDirectoryService.Setup(d => d.CreateDirectory(dummyCacheDirectory)).Throws(dummyException);
            DiskCacheService testSubject = CreateDiskCacheService(directoryService: mockDirectoryService.Object);

            // Act and assert
            InvalidOperationException result = Assert.Throws<InvalidOperationException>(() => testSubject.CreateOrGetCacheFile(dummySource, dummyCacheDirectory));

            // Assert
            _mockRepository.VerifyAll();
            Assert.Equal(string.Format(Strings.InvalidOperationException_DiskCacheService_InvalidDiskCacheDirectory, dummyCacheDirectory), result.Message);
            Assert.Same(dummyException, result.InnerException);
        }

        [Fact]
        public void CreateOrGetCacheFile_ReturnsFileStreamIfSuccessful()
        {
            // Arrange
            const string dummyFilePath = "dummyFilePath";
            const string dummySource = "dummySource";
            const string dummyCacheDirectory = "dummyCacheDirectory";
            Mock<IDirectoryService> mockDirectoryService = _mockRepository.Create<IDirectoryService>();
            mockDirectoryService.Setup(d => d.CreateDirectory(dummyCacheDirectory)); // Do nothing
            using (FileStream dummyFileStream = File.Open(_dummyFile, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite))
            {
                Mock<DiskCacheService> mockTestSubject = CreateMockDiskCacheService(directoryService: mockDirectoryService.Object);
                mockTestSubject.CallBase = true;
                mockTestSubject.Setup(t => t.CreatePath(dummySource, dummyCacheDirectory)).Returns(dummyFilePath);
                mockTestSubject.Setup(t => t.GetStream(dummyFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)).Returns(dummyFileStream);

                // Act
                FileStream result = mockTestSubject.Object.CreateOrGetCacheFile(dummySource, dummyCacheDirectory);

                // Assert
                _mockRepository.VerifyAll();
                Assert.Same(dummyFileStream, result);
            }
        }

        [Fact]
        public void CreateOrGetCacheFile_ThrowsInvalidOperationExceptionIfAnUnexpectedExceptionIsThrownWhenAttemptingToOpenAStream()
        {
            // Arrange
            const string dummyFilePath = "dummyFilePath";
            const string dummySource = "dummySource";
            const string dummyCacheDirectory = "dummyCacheDirectory";
            Mock<IDirectoryService> mockDirectoryService = _mockRepository.Create<IDirectoryService>();
            mockDirectoryService.Setup(d => d.CreateDirectory(dummyCacheDirectory)); // Do nothing
            var dummyException = new IOException();
            Mock<DiskCacheService> mockTestSubject = CreateMockDiskCacheService(directoryService: mockDirectoryService.Object);
            mockTestSubject.CallBase = true;
            mockTestSubject.Setup(t => t.CreatePath(dummySource, dummyCacheDirectory)).Returns(dummyFilePath);
            mockTestSubject.Setup(t => t.GetStream(dummyFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)).Throws(dummyException);

            // Act and assert
            InvalidOperationException result = Assert.Throws<InvalidOperationException>(() => mockTestSubject.Object.CreateOrGetCacheFile(dummySource, dummyCacheDirectory));

            // Assert
            _mockRepository.VerifyAll();
            Assert.Equal(string.Format(Strings.InvalidOperationException_DiskCacheService_UnexpectedDiskCacheException, dummySource, dummyFilePath), result.Message);
            Assert.Same(dummyException, result.InnerException);
        }

        [Fact]
        public void GetStream_LogsWarningsThenThrowsIOExceptionIfCacheFileExistsButIsInUseAndRemainsInUseOnTheThirdTryToOpenIt()
        {
            // Arrange
            const string dummyFilePath = "dummyFilePath";
            const FileMode dummyFileMode = FileMode.Open;
            const FileAccess dummyFileAccess = FileAccess.Read;
            const FileShare dummyFileShare = FileShare.Read;
            var dummyIOException = new IOException();
            Mock<ILogger> mockLogger = _mockRepository.Create<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Warning)).Returns(true);
            Mock<ILoggerFactory> mockLoggerFactory = _mockRepository.Create<ILoggerFactory>();
            mockLoggerFactory.Setup(l => l.CreateLogger(typeof(DiskCacheService).FullName)).Returns(mockLogger.Object);
            Mock<IFileService> mockFileService = _mockRepository.Create<IFileService>();
            mockFileService.Setup(f => f.Open(dummyFilePath, dummyFileMode, dummyFileAccess, dummyFileShare)).Throws(dummyIOException);
            DiskCacheService testSubject = CreateDiskCacheService(fileService: mockFileService.Object, loggerFactory: mockLoggerFactory.Object);

            // Act and assert
            IOException result = Assert.Throws<IOException>(() => testSubject.GetStream(dummyFilePath, dummyFileMode, dummyFileAccess, dummyFileShare));
            _mockRepository.VerifyAll();
            mockFileService.Verify(f => f.Open(dummyFilePath, dummyFileMode, dummyFileAccess, dummyFileShare), Times.Exactly(3));
            Assert.Same(dummyIOException, result);
            mockLogger.Verify(l => l.Log(LogLevel.Warning, 0,
                    // object is of type FormattedLogValues
                    It.Is<It.IsAnyType>((f, _) => f.ToString() == string.Format(Strings.LogWarning_DiskCacheService_FileInUse, dummyFilePath, 2)),
                    null,
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
            mockLogger.Verify(l => l.Log(LogLevel.Warning, 0,
                    It.Is<It.IsAnyType>((f, _) => f.ToString() == string.Format(Strings.LogWarning_DiskCacheService_FileInUse, dummyFilePath, 1)),
                    null,
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
            mockLogger.Verify(l => l.Log(LogLevel.Warning, 0,
                    It.Is<It.IsAnyType>((f, _) => f.ToString() == string.Format(Strings.LogWarning_DiskCacheService_FileInUse, dummyFilePath, 0)),
                    null,
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        [Fact]
        public void CreatePath_CreatesPath()
        {
            // Arrange 
            const string dummyCacheDirectory = "dummyCacheDirectory";
            const string dummySource = "dummySource";
            const string dummyCacheIdentifier = "dummyCacheIdentifier";
            Mock<DiskCacheService> mockTestSubject = CreateMockDiskCacheService();
            mockTestSubject.CallBase = true;
            mockTestSubject.Setup(t => t.GetCacheIdentifier(dummySource)).Returns(dummyCacheIdentifier);

            // Act
            string result = mockTestSubject.Object.CreatePath(dummySource, dummyCacheDirectory);
            Assert.Equal($"{dummyCacheDirectory}{Path.DirectorySeparatorChar}{dummyCacheIdentifier}.txt", result);
        }

        [Fact]
        public void GetCacheIdentifier_GetsIdentifier()
        {
            // Arrange
            const string dummyAbsoluteUri = "file:///host/dummy/absolute/path";
            DiskCacheService testSubject = CreateDiskCacheService();

            // Act
            string result = testSubject.GetCacheIdentifier(dummyAbsoluteUri);

            // Assert
            Assert.Equal("19AB76B0543B5B7F9707E392C5C5EE47", result);
        }

        private Mock<DiskCacheService> CreateMockDiskCacheService(IFileService fileService = null, IDirectoryService directoryService = null, ILoggerFactory loggerFactory = null)
        {
            return _mockRepository.Create<DiskCacheService>(fileService ?? _mockRepository.Create<IFileService>().Object,
                directoryService ?? _mockRepository.Create<IDirectoryService>().Object,
                loggerFactory ?? _mockRepository.Create<ILoggerFactory>().Object);
        }

        private DiskCacheService CreateDiskCacheService(IFileService fileService = null, IDirectoryService directoryService = null, ILoggerFactory loggerFactory = null)
        {
            return new DiskCacheService(fileService ?? _mockRepository.Create<IFileService>().Object,
                directoryService ?? _mockRepository.Create<IDirectoryService>().Object,
                loggerFactory ?? _mockRepository.Create<ILoggerFactory>().Object);
        }
    }

    public class DiskCacheServiceUnitTestsFixture : IDisposable
    {
        public string TempDirectory { get; } = Path.Combine(Path.GetTempPath(), nameof(DiskCacheServiceUnitTests)); // Dummy file for creating dummy file streams

        public DiskCacheServiceUnitTestsFixture()
        {
            TryDeleteDirectory();
            Directory.CreateDirectory(TempDirectory);
        }

        private void TryDeleteDirectory()
        {
            try
            {
                Directory.Delete(TempDirectory, true);
            }
            catch
            {
                // Do nothing
            }
        }

        public void Dispose()
        {
            TryDeleteDirectory();
        }
    }
}
