using AppInMeeting.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppInMeeting.Services
{
    public class TasksService
    {
        private readonly ConcurrentDictionary<string, List<TaskInfoModel>> todoDictionary
        = new ConcurrentDictionary<string, List<TaskInfoModel>>();

        private readonly ConcurrentDictionary<string, List<TaskInfoModel>> doingDictionary
        = new ConcurrentDictionary<string, List<TaskInfoModel>>();

        private readonly ConcurrentDictionary<string, List<TaskInfoModel>> doneDictionary
        = new ConcurrentDictionary<string, List<TaskInfoModel>>();

        public ConcurrentDictionary<string, List<TaskInfoModel>> ToDoDictionary => todoDictionary;
        public ConcurrentDictionary<string, List<TaskInfoModel>> DoingDictionary => doingDictionary;
        public ConcurrentDictionary<string, List<TaskInfoModel>> DoneDictionary => doneDictionary;

    }
}
