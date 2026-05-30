using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace AISDisciplineDesc.Services
{
    public interface IUsbAuthService
    {
        bool IsValidKeyPresent(string expectedSerial = null);
    }

    public class FlashDriveInfo
    {
        public string DriveLetter { get; set; }
        public string VolumeLabel { get; set; }
        public string SerialNumber { get; set; }
    }

    public class UsbAuthService : IUsbAuthService
    {
        private const string KeyFileName = ".authkey";
        private readonly string _expectedKey;

        public UsbAuthService(string expectedKey)
        {
            _expectedKey = expectedKey;
        }

        /// <summary>
        /// Проверяет наличие файла .authkey и, если задан expectedSerial, совпадение серийного номера.
        /// </summary>
        public bool IsValidKeyPresent(string expectedSerial = null)
        {
            return DriveInfo.GetDrives()
                .Where(d => d.DriveType == DriveType.Removable && d.IsReady)
                .Any(d =>
                {
                    // Проверяем файл ключ
                    string keyPath = Path.Combine(d.RootDirectory.FullName, KeyFileName);
                    if (!File.Exists(keyPath)) return false;
                    string content = File.ReadAllText(keyPath).Trim();
                    if (content != _expectedKey) return false;

                    // Если серийный номер не требуется, флешка подходит
                    if (string.IsNullOrEmpty(expectedSerial))
                    {
                        return true;
                    }
                    // Проверяем серийный номер устройства
                    string serial = GetDriveSerial(d.Name[0]);
                    return serial == expectedSerial;
                });
        }

        private string GetDriveSerial(char driveLetter) //Метод вытаскивающий серийный номер флешки для присвоения аккаунту
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    $"SELECT * FROM Win32_LogicalDisk WHERE DeviceID='{driveLetter}:'");
                foreach (ManagementObject logical in searcher.Get())
                {
                    var query = $"ASSOCIATORS OF {{Win32_LogicalDisk.DeviceID='{logical["DeviceID"]}'}} WHERE ResultClass=Win32_DiskDrive";
                    using var driveSearcher = new ManagementObjectSearcher(query);
                    foreach (ManagementObject disk in driveSearcher.Get())
                    {
                        string hwSerial = disk["SerialNumber"]?.ToString()?.Trim();
                        if (!string.IsNullOrEmpty(hwSerial))
                            return hwSerial;
                    }

                    string volSerial = logical["VolumeSerialNumber"]?.ToString()?.Trim();
                    if (!string.IsNullOrEmpty(volSerial))
                        return "VOL:" + volSerial;
                }
            }
            catch { }

            return "";
        }

        public List<FlashDriveInfo> GetAllAuthFlashDrives() //Поиск всех флешек на компьютере для будущего присвоения ключа пользователю
        {
            var list = new List<FlashDriveInfo>();
            foreach (var d in DriveInfo.GetDrives())
            {
                if (d.DriveType != DriveType.Removable || !d.IsReady)
                    continue;

                string keyPath = Path.Combine(d.RootDirectory.FullName, KeyFileName);
                if (File.Exists(keyPath))
                {
                    list.Add(new FlashDriveInfo
                    {
                        DriveLetter = d.Name.TrimEnd('\\'),
                        VolumeLabel = d.VolumeLabel,
                        SerialNumber = GetDriveSerial(d.Name[0])
                    });
                }
            }
            return list;
        }
    }
}
