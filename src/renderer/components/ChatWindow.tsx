import React, { useEffect, useState, useRef } from 'react';
import { supabase } from '@/lib/supabase';
import { useAuthStore } from '@/hooks/useAuth';
import MessageInput from './MessageInput';

interface Message {
  id: string;
  session_id: string;
  sender_id: string;
  content: string;
  created_at: string;
  sender_email?: string;
}

interface ChatWindowProps {
  sessionId: string;
}

const ChatWindow: React.FC<ChatWindowProps> = ({ sessionId }) => {
  const [messages, setMessages] = useState<Message[]>([]);
  const [sessionInfo, setSessionInfo] = useState<any>(null);
  const [loading, setLoading] = useState(true);
  const user = useAuthStore((state) => state.user);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  useEffect(() => {
    if (!sessionId || !user) return;

    const fetchSessionData = async () => {
      setLoading(true);
      try {
        // Fetch session details
        const { data: sessionData, error: sessionError } = await supabase
          .from('chat_sessions')
          .select('*')
          .eq('id', sessionId)
          .single();

        if (sessionError) throw sessionError;
        setSessionInfo(sessionData);

        // Fetch messages with sender email
        const { data: messagesData, error: messagesError } = await supabase
          .from('messages')
          .select('*, users!messages_sender_id_fkey(email)')
          .eq('session_id', sessionId)
          .order('created_at', { ascending: true });

        if (messagesError) throw messagesError;

        const formattedMessages = messagesData.map((m: any) => ({
          ...m,
          sender_email: m.users?.email
        }));

        setMessages(formattedMessages);
        setTimeout(scrollToBottom, 100);
      } catch (err) {
        console.error('Error fetching chat data:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchSessionData();

    // Subscribe to new messages
    const subscription = supabase
      .channel(`messages:${sessionId}`)
      .on('postgres_changes', 
        { event: 'INSERT', schema: 'public', table: 'messages', filter: `session_id=eq.${sessionId}` },
        async (payload) => {
          // Fetch sender info for the new message
          const { data: senderData } = await supabase
            .from('users')
            .select('email')
            .eq('id', payload.new.sender_id)
            .single();

          const newMessage = {
            ...payload.new as Message,
            sender_email: senderData?.email
          };
          
          setMessages((prev) => [...prev, newMessage]);
          setTimeout(scrollToBottom, 100);
        }
      )
      .subscribe();

    return () => {
      subscription.unsubscribe();
    };
  }, [sessionId, user]);

  const handleSendMessage = async (content: string) => {
    if (!sessionId || !user || !content.trim()) return;

    try {
      const { error } = await supabase.from('messages').insert({
        session_id: sessionId,
        sender_id: user.id,
        content: content.trim(),
      });

      if (error) throw error;
      
      // Update session last_message_at
      await supabase
        .from('chat_sessions')
        .update({ last_message_at: new Date().toISOString() })
        .eq('id', sessionId);
        
    } catch (err) {
      console.error('Error sending message:', err);
    }
  };

  if (loading) {
    return <div className="flex items-center justify-center flex-1 bg-gray-50 text-gray-400">正在加载聊天...</div>;
  }

  return (
    <div className="flex flex-col h-full bg-[#F5F5F5]">
      {/* Chat Header */}
      <header className="px-6 py-4 bg-white border-b border-gray-200 flex items-center justify-between">
        <h2 className="text-lg font-bold text-gray-800">
          {sessionInfo?.name || '聊天'}
        </h2>
      </header>

      {/* Messages List */}
      <div className="flex-1 overflow-y-auto p-6 space-y-4">
        {messages.map((msg) => {
          const isMine = msg.sender_id === user?.id;
          return (
            <div
              key={msg.id}
              className={`flex ${isMine ? 'justify-end' : 'justify-start'}`}
            >
              <div
                className={`flex max-w-[70%] ${
                  isMine ? 'flex-row-reverse' : 'flex-row'
                } items-start space-x-2`}
              >
                <div className={`w-8 h-8 rounded-full bg-gray-400 flex-shrink-0 flex items-center justify-center text-xs font-bold text-white ${isMine ? 'ml-2' : 'mr-2'}`}>
                  {msg.sender_email?.[0].toUpperCase() || 'U'}
                </div>
                <div className="flex flex-col">
                  {!isMine && (
                    <span className="text-[10px] text-gray-500 mb-1 ml-1">
                      {msg.sender_email}
                    </span>
                  )}
                  <div
                    className={`px-4 py-2 rounded-lg text-sm shadow-sm ${
                      isMine
                        ? 'bg-[#95EC69] text-gray-800 rounded-tr-none'
                        : 'bg-white text-gray-800 rounded-tl-none'
                    }`}
                  >
                    {msg.content}
                  </div>
                  <span className={`text-[10px] text-gray-400 mt-1 ${isMine ? 'text-right' : 'text-left'}`}>
                    {new Date(msg.created_at).toLocaleTimeString([], {
                      hour: '2-digit',
                      minute: '2-digit',
                    })}
                  </span>
                </div>
              </div>
            </div>
          );
        })}
        <div ref={messagesEndRef} />
      </div>

      {/* Message Input */}
      <MessageInput onSend={handleSendMessage} />
    </div>
  );
};

export default ChatWindow;
