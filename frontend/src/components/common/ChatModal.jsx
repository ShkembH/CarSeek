import React, { useEffect, useState, useRef } from "react";
import * as signalR from "@microsoft/signalr";
import axios from "axios";

const API_BASE = process.env.REACT_APP_API_URL || "http://localhost:5193/api";

const ChatModal = ({ ownerId, listingId, onClose, currentUserId, token, otherUserName }) => {
  console.log('ChatModal mounted', { ownerId, listingId, currentUserId });
  const [connection, setConnection] = useState(null);
  const [messages, setMessages] = useState([]);
  const [input, setInput] = useState("");
  const [loading, setLoading] = useState(true);
  const messagesEndRef = useRef(null);
  const connectionRef = useRef(null);

  // Scroll to bottom on new message
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages]);

  // Fetch chat history
  useEffect(() => {
    const fetchHistory = async () => {
      setLoading(true);
      try {
        const res = await axios.get(
          `${API_BASE}/Chat/history`,
          {
            params: { userId: ownerId, listingId },
            headers: { Authorization: `Bearer ${token}` },
          }
        );
        setMessages(res.data || []);
      } catch (err) {
        // handle error
      }
      setLoading(false);
    };
    if (ownerId && listingId) fetchHistory();
  }, [ownerId, listingId, token]);

  // SignalR connection
  useEffect(() => {
    let isMounted = true;
    // Always use HTTP for localhost:5193 to avoid SSL issues
    const chatHubUrl = window.location.hostname === "localhost"
      ? "http://localhost:5193/chathub"
      : `${API_BASE.replace("/api", "")}/chathub`;
    const conn = new signalR.HubConnectionBuilder()
      .withUrl(chatHubUrl, {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect()
      .build();
    connectionRef.current = conn;

    conn.on("ReceiveMessage", (fromUserId, message, listingIdMsg) => {
      if (listingIdMsg === listingId && isMounted) {
        setMessages((msgs) => [
          ...msgs,
          {
            senderId: fromUserId,
            recipientId: currentUserId,
            message,
            listingId: listingIdMsg,
            createdAt: new Date().toISOString(),
          },
        ]);
      }
    });

    conn.start()
      .then(() => {
        if (isMounted) setConnection(conn);
      })
      .catch((err) => {
        if (
          err.message &&
          err.message.includes("The connection was stopped during negotiation")
        ) {
          // Suppress this warning in development
          if (process.env.NODE_ENV === "development") return;
        }
        // Optionally log other errors
        console.error("SignalR connection error:", err);
      });
    return () => {
      isMounted = false;
      conn.stop();
      connectionRef.current = null;
    };
  }, [ownerId, listingId, token, currentUserId]);

  // Mark messages as read when modal opens
  useEffect(() => {
    const markRead = async () => {
      try {
        await axios.post(
          `${API_BASE}/Chat/mark-read`,
          { userId: ownerId, listingId },
          { headers: { Authorization: `Bearer ${token}` } }
        );
      } catch {}
    };
    if (ownerId && listingId) markRead();
  }, [ownerId, listingId, token]);

  const sendMessage = async () => {
    if (connection && input.trim()) {
      await connection.invoke("SendMessage", ownerId, input, listingId);
      setMessages((msgs) => [
        ...msgs,
        {
          senderId: currentUserId,
          recipientId: ownerId,
          message: input,
          listingId,
          createdAt: new Date().toISOString(),
        },
      ]);
      setInput("");
    }
  };

  return (
    <div className="chat-embedded">
      <div className="chat-header">
        <span>{otherUserName ? `Chat with ${otherUserName}` : "Chat"}</span>
        <button onClick={onClose} className="chat-close-btn">×</button>
      </div>
      <div className="chat-body">
        {loading ? (
          <div>Loading...</div>
        ) : (
          messages.map((msg, idx) => (
            <div
              key={idx}
              className={
                msg.senderId === currentUserId ? "my-message" : "their-message"
              }
            >
              {msg.message}
              <div className="chat-timestamp">
                {new Date(msg.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
              </div>
            </div>
          ))
        )}
        <div ref={messagesEndRef} />
      </div>
      <div className="chat-footer">
        <input
          value={input}
          onChange={e => setInput(e.target.value)}
          onKeyDown={e => e.key === "Enter" && sendMessage()}
          placeholder="Type a message..."
        />
        <button onClick={sendMessage} disabled={!input.trim()}>Send</button>
      </div>
      <style>{`
        .chat-embedded {
          background: #fff;
          width: 100%;
          height: 100%;
          display: flex;
          flex-direction: column;
          color: #222;
          border: none;
        }
        .chat-header {
          padding: 1rem;
          border-bottom: 1px solid #eee;
          display: flex;
          justify-content: space-between;
          align-items: center;
          font-weight: bold;
        }
        .chat-close-btn {
          background: none;
          border: none;
          font-size: 1.5rem;
          cursor: pointer;
        }
        .chat-body {
          flex: 1;
          overflow-y: auto;
          padding: 1rem;
          background: #f9f9f9;
          display: flex;
          flex-direction: column;
        }
        .my-message {
          background: #e6f7ff;
          align-self: flex-end;
          margin: 0.3rem 0;
          padding: 0.5rem 0.8rem;
          border-radius: 15px 15px 2px 15px;
          max-width: 70%;
          text-align: right;
        }
        .their-message {
          background: #f1f1f1;
          align-self: flex-start;
          margin: 0.3rem 0;
          padding: 0.5rem 0.8rem;
          border-radius: 15px 15px 15px 2px;
          max-width: 70%;
          text-align: left;
        }
        .chat-timestamp {
          font-size: 0.7rem;
          color: #888;
          margin-top: 2px;
          text-align: right;
        }
        .chat-footer {
          display: flex;
          border-top: 1px solid #eee;
          padding: 0.7rem 1rem;
          background: #fff;
        }
        .chat-footer input {
          flex: 1;
          border: 1px solid #ddd;
          border-radius: 20px;
          padding: 0.5rem 1rem;
          margin-right: 0.5rem;
        }
        .chat-footer button {
          background: #1890ff;
          color: #fff;
          border: none;
          border-radius: 20px;
          padding: 0.5rem 1.2rem;
          font-weight: bold;
          cursor: pointer;
        }
      `}</style>
    </div>
  );
};

export default ChatModal;
