namespace AutoGame.Core.Interfaces;

using System.Threading.Tasks;
using AutoGame.Core.Models;

public interface IUpdateCheckingService
{
    Task<UpdateInfo> GetUpdateInfo();
}