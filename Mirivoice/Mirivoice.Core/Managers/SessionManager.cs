using Avalonia.Controls.Documents;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Mirivoice.Engines;
using Mirivoice.Engines.TTS;
using Mirivoice.Mirivoice.Core.Format;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirivoice.Mirivoice.Core.Managers
{
    public class SessionCache
    {
        public string ModelPath { get; set; }
        public InferenceSession Session { get; set; }
        public DateTime LastAccessed { get; set; }
    }
    public class SessionManager
    {
        private readonly List<SessionCache> _sessionPool = new List<SessionCache>();
        public SessionManager()
        {
        }

        public void SetNewMaxSessionCount(int newMaxCount)
        {
            if (_sessionPool.Count >= newMaxCount)
            {
                // handle case that user changed the session count limit in settings, and the current session pool exceeds the new limit
                while (_sessionPool.Count >= newMaxCount)
                {
                    var oldest = _sessionPool.OrderBy(s => s.LastAccessed).First();
                    oldest.Session.Dispose();
                    _sessionPool.Remove(oldest);
                    Log.Information($"Session for model {oldest.ModelPath} has been disposed due to max session count limit. current session count: {_sessionPool.Count}, new max session count: {newMaxCount}");
                }
            }
        }

        public InferenceSession GetSession(string modelPath, BaseConfig config, BaseEngine engine)
        {
            Log.Information($"[SessionManager] Requesting session for model: {modelPath}, max session count: {MainManager.Instance.Setting.MaxSessionCount}");
            lock (_sessionPool)
            {
                // check if the session already exists in the pool
                var cached = _sessionPool.FirstOrDefault(s => s.ModelPath == modelPath);

                if (cached != null)
                {
                    // update last accessed time for this session
                    cached.LastAccessed = DateTime.UtcNow;
                    return cached.Session;
                }

                // create a new session and add it to the pool
                if (_sessionPool.Count >= MainManager.Instance.Setting.MaxSessionCount)
                {
                    var oldest = _sessionPool.OrderBy(s => s.LastAccessed).First();
                    oldest.Session.Dispose();
                    _sessionPool.Remove(oldest);
                    Log.Information($"Session for model {oldest.ModelPath} has been disposed due to max session count limit.");
                }

                Log.Information($"[SessionManager] Loading model from path: {modelPath}");
                var options = new SessionOptions();
                options.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;

                var newSession = new InferenceSession(modelPath, options);
                engine.WarmupSession(newSession);

                _sessionPool.Add(new SessionCache
                {
                    ModelPath = modelPath,
                    Session = newSession,
                    LastAccessed = DateTime.UtcNow
                });

                return newSession;
            }
        } 

        public void ClearSessions()
        {
            lock (_sessionPool)
            {
                foreach (var sessionCache in _sessionPool)
                {
                    sessionCache.Session.Dispose();
                }
                _sessionPool.Clear();
                Log.Information($"All sessions have been cleared.");
            }
        }
    }
}
