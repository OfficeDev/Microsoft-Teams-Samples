// <copyright file="TaskModuleModels.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace meetings_transcription.Models
{
    /// <summary>
    /// Task module response.
    /// </summary>
    public class TaskModuleResponse
    {
        /// <summary>
        /// Gets or sets the task.
        /// </summary>
        public TaskModuleContinueResponse Task { get; set; }
    }

    /// <summary>
    /// Task module continue response.
    /// </summary>
    public class TaskModuleContinueResponse
    {
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public TaskModuleTaskInfo Value { get; set; }
    }

    /// <summary>
    /// Task module task info.
    /// </summary>
    public class TaskModuleTaskInfo
    {
        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }
    }
}
