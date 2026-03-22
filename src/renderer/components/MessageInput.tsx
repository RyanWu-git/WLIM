import React, { useState, useRef, useEffect } from 'react';
import { Send, Smile, Paperclip } from 'lucide-react';

interface MessageInputProps {
  onSend: (content: string) => void;
}

const MessageInput: React.FC<MessageInputProps> = ({ onSend }) => {
  const [content, setContent] = useState('');
  const textareaRef = useRef<HTMLTextAreaElement>(null);

  const handleSend = () => {
    if (content.trim()) {
      onSend(content);
      setContent('');
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  // Auto-resize textarea
  useEffect(() => {
    const textarea = textareaRef.current;
    if (textarea) {
      textarea.style.height = 'auto';
      textarea.style.height = `${Math.min(textarea.scrollHeight, 120)}px`;
    }
  }, [content]);

  return (
    <div className="px-6 py-4 bg-white border-t border-gray-200">
      {/* Tool Icons */}
      <div className="flex space-x-4 mb-2">
        <button className="text-gray-400 hover:text-blue-500 transition-colors">
          <Smile size={20} />
        </button>
        <button className="text-gray-400 hover:text-blue-500 transition-colors">
          <Paperclip size={20} />
        </button>
      </div>

      <div className="flex items-end space-x-3">
        <textarea
          ref={textareaRef}
          value={content}
          onChange={(e) => setContent(e.target.value)}
          onKeyDown={handleKeyDown}
          className="flex-1 px-4 py-2 text-sm text-gray-800 bg-gray-50 border border-transparent rounded-lg focus:ring-2 focus:ring-blue-500 focus:bg-white focus:border-blue-500 outline-none transition-all resize-none max-h-[120px]"
          placeholder="请输入消息... (Enter 发送)"
          rows={1}
        />
        <button
          onClick={handleSend}
          disabled={!content.trim()}
          className={`p-2 rounded-lg transition-all ${
            content.trim()
              ? 'bg-blue-600 text-white hover:bg-blue-700'
              : 'bg-gray-200 text-gray-400 cursor-not-allowed'
          }`}
        >
          <Send size={20} />
        </button>
      </div>
    </div>
  );
};

export default MessageInput;
