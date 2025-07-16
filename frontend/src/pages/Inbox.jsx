import React, { useEffect, useState } from "react";
import axios from "axios";
import { useAuth } from "../context/AuthContext";
import ChatModal from "../components/common/ChatModal";

const API_BASE = process.env.REACT_APP_API_URL || "http://localhost:5193/api";

const Inbox = () => {
  const { user, isAuthenticated } = useAuth();
  const [conversations, setConversations] = useState([]);
  const [selected, setSelected] = useState(null); // { otherUserId, listingId, otherUserName }
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [userInfoMap, setUserInfoMap] = useState({}); // userId -> { firstName, lastName, role, companyName }
  const [listingInfoMap, setListingInfoMap] = useState({}); // listingId -> { title }

  useEffect(() => {
    const fetchConversations = async () => {
      setLoading(true);
      try {
        const res = await axios.get(`${API_BASE}/Chat/conversations`, {
          headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
        });
        setConversations(res.data || []);
        // Fetch user info for all unique otherUserIds
        const uniqueUserIds = [...new Set((res.data || []).map(c => c.otherUserId))];
        const userInfoResults = await Promise.all(
          uniqueUserIds.map(async (id) => {
            try {
              const infoRes = await axios.get(`${API_BASE}/Chat/user-info`, {
                params: { userId: id },
                headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
              });
              return [id, infoRes.data];
            } catch {
              return [id, null];
            }
          })
        );
        const infoMap = {};
        userInfoResults.forEach(([id, info]) => {
          if (info) infoMap[id] = info;
        });
        setUserInfoMap(infoMap);
        // Fetch listing info for all unique listingIds
        const uniqueListingIds = [...new Set((res.data || []).map(c => c.listingId))];
        const listingInfoResults = await Promise.all(
          uniqueListingIds.map(async (id) => {
            try {
              const infoRes = await axios.get(`${API_BASE}/CarListings/${id}`);
              return [id, infoRes.data];
            } catch {
              return [id, null];
            }
          })
        );
        const listingMap = {};
        listingInfoResults.forEach(([id, info]) => {
          if (info) listingMap[id] = info;
        });
        setListingInfoMap(listingMap);
      } catch (err) {
        setError("Failed to load conversations");
      }
      setLoading(false);
    };
    if (isAuthenticated) fetchConversations();
  }, [isAuthenticated]);

  // Helper to get the other user's display name
  const getOtherUserName = (conv) => {
    const info = userInfoMap[conv.otherUserId];
    if (!info) return "Unknown User";
    if (info.role && info.role.toLowerCase() === "dealership" && info.companyName) return info.companyName;
    return `${info.firstName} ${info.lastName}`;
  };

  // Helper to get the listing title
  const getListingTitle = (conv) => {
    const info = listingInfoMap[conv.listingId];
    return info ? info.title : conv.listingId.slice(0, 8) + '...';
  };

  return (
    <div className="inbox-page">
      <div className="inbox-sidebar">
        <h2>Inbox</h2>
        {loading ? (
          <div>Loading...</div>
        ) : error ? (
          <div className="error">{error}</div>
        ) : conversations.length === 0 ? (
          <div>No conversations yet.</div>
        ) : (
          <ul className="inbox-list">
            {conversations.map((conv, idx) => (
              <li
                key={conv.listingId + conv.otherUserId}
                className={
                  selected &&
                  selected.otherUserId === conv.otherUserId &&
                  selected.listingId === conv.listingId
                    ? "selected"
                    : ""
                }
                onClick={() =>
                  setSelected({
                    otherUserId: conv.otherUserId,
                    listingId: conv.listingId,
                    otherUserName: getOtherUserName(conv),
                  })
                }
              >
                <div className="inbox-user-name">{getOtherUserName(conv)}</div>
                <div className="inbox-listing-id">Listing: {getListingTitle(conv)}</div>
                <div className="inbox-last-message">{conv.lastMessage?.message?.slice(0, 30) || "(No messages)"}</div>
                {conv.unreadCount > 0 && (
                  <span className="inbox-unread">{conv.unreadCount}</span>
                )}
              </li>
            ))}
          </ul>
        )}
      </div>
      <div className="inbox-chat-area">
        {selected ? (
          <ChatModal
            ownerId={selected.otherUserId}
            listingId={selected.listingId}
            onClose={() => setSelected(null)}
            currentUserId={user.id}
            token={localStorage.getItem("token")}
            otherUserName={selected.otherUserName}
          />
        ) : (
          <div className="inbox-placeholder">Select a conversation to start chatting.</div>
        )}
      </div>
      <style>{`
        .inbox-page {
          display: flex;
          height: 90vh;
          background: #f5f6fa;
        }
        .inbox-sidebar {
          width: 20%;
          min-width: 220px;
          background: #fff;
          border-right: 1px solid #eee;
          padding: 1rem 0.5rem;
          box-sizing: border-box;
          overflow-y: auto;
        }
        .inbox-sidebar h2 {
          margin: 0 0 1rem 0.5rem;
          font-size: 1.3rem;
        }
        .inbox-list {
          list-style: none;
          padding: 0;
          margin: 0;
        }
        .inbox-list li {
          padding: 0.7rem 0.7rem 0.7rem 0.7rem;
          border-radius: 8px;
          margin-bottom: 0.3rem;
          cursor: pointer;
          background: #f7f7f7;
          transition: background 0.2s;
          position: relative;
        }
        .inbox-list li.selected, .inbox-list li:hover {
          background: #e6f0ff;
        }
        .inbox-user-name {
          font-weight: bold;
          font-size: 1rem;
        }
        .inbox-listing-id {
          font-size: 0.8rem;
          color: #888;
        }
        .inbox-last-message {
          font-size: 0.9rem;
          color: #444;
          margin-top: 2px;
        }
        .inbox-unread {
          background: #ff4d4f;
          color: #fff;
          border-radius: 10px;
          padding: 2px 8px;
          font-size: 0.8rem;
          position: absolute;
          right: 10px;
          top: 10px;
        }
        .inbox-chat-area {
          width: 80%;
          display: flex;
          align-items: center;
          justify-content: center;
          background: #f5f6fa;
        }
        .inbox-placeholder {
          color: #888;
          font-size: 1.2rem;
        }
      `}</style>
    </div>
  );
};

export default Inbox;
