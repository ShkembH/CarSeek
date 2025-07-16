import React, { useState, useRef, useEffect } from 'react';
import { useAuth } from '../../context/AuthContext';
import '../../styles/components/Header.css';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';

const Header = () => {
  const { user, logout, isAuthenticated, isAdmin } = useAuth();
  const [isDropdownOpen, setIsDropdownOpen] = useState(false);
  const [isBuyDropdownOpen, setIsBuyDropdownOpen] = useState(false);
  const dropdownRef = useRef(null);
  const buyDropdownRef = useRef(null);
  const [showHeader, setShowHeader] = useState(true);
  const lastScrollY = useRef(window.scrollY);
  const navigate = useNavigate();
  const [unreadCount, setUnreadCount] = useState(0);

  // Car makes for Buy dropdown
  const carMakes = ['Toyota', 'BMW', 'Mercedes', 'Honda', 'Audi', 'Volkswagen', 'Ford', 'Chevrolet'];

  const handleMakeClick = (make) => {
    navigate(`/listings?make=${encodeURIComponent(make)}`);
    setIsBuyDropdownOpen(false);
  };

  useEffect(() => {
    const handleScroll = () => {
      if (window.scrollY < 40) {
        setShowHeader(true);
        lastScrollY.current = window.scrollY;
        return;
      }
      if (window.scrollY < lastScrollY.current) {
        setShowHeader(true); // Scrolling up
      } else if (window.scrollY > lastScrollY.current) {
        setShowHeader(false); // Scrolling down
      }
      lastScrollY.current = window.scrollY;
    };
    window.addEventListener('scroll', handleScroll);
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);

  useEffect(() => {
    const fetchUnread = async () => {
      if (!isAuthenticated) return;
      try {
        const res = await axios.get((process.env.REACT_APP_API_URL || 'http://localhost:5193/api') + '/Chat/conversations', {
          headers: { Authorization: `Bearer ${localStorage.getItem('token')}` },
        });
        const totalUnread = (res.data || []).reduce((sum, c) => sum + (c.unreadCount || 0), 0);
        setUnreadCount(totalUnread);
      } catch {}
    };
    fetchUnread();
  }, [isAuthenticated]);

  const handleLogout = () => {
    logout();
    window.location.href = '/';
  };

  const toggleDropdown = () => {
    setIsDropdownOpen(!isDropdownOpen);
  };

  const toggleBuyDropdown = () => {
    setIsBuyDropdownOpen(!isBuyDropdownOpen);
  };

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target)) {
        setIsDropdownOpen(false);
      }
      if (buyDropdownRef.current && !buyDropdownRef.current.contains(event.target)) {
        setIsBuyDropdownOpen(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, []);

  // Generate initials for profile picture
  const getInitials = (firstName, lastName) => {
    if (firstName && lastName) {
      return `${firstName.charAt(0)}${lastName.charAt(0)}`.toUpperCase();
    } else if (firstName) {
      return firstName.charAt(0).toUpperCase();
    }
    return 'U';
  };

  return (
    <header className={`header${showHeader ? ' header-slide-down' : ' header-slide-up'}`}>
      <div className="header-container">
        {/* Left: Logo */}
        <div className="header-logo">
          <a href="/" className="header-logo-link">
            <span className="header-logo-text">Car</span>
            <span className="header-logo-accent">Seek</span>
          </a>
        </div>

        {/* Center: Navigation */}
        <nav className="header-nav">
          <a href="/" className="header-nav-link">Home</a>
          <a href="/listings" className="header-nav-link">Listings</a>
          {/* Buy Dropdown */}
          <div className="header-buy-dropdown" ref={buyDropdownRef}>
            <button onClick={toggleBuyDropdown} className="header-nav-link header-buy-btn">
              Buy ▼
            </button>
            {isBuyDropdownOpen && (
              <div className="header-buy-dropdown-menu">
                {carMakes.map(make => (
                  <button
                    key={make}
                    className="header-buy-dropdown-item"
                    onClick={() => handleMakeClick(make)}
                  >
                    {make}
                  </button>
                ))}
              </div>
            )}
          </div>
        </nav>

        {/* Right: Auth/User Actions */}
        <div className="header-right-section">
          {isAuthenticated ? (
            <div className="header-user-actions">
              {/* Only show dashboard link for admin users */}
              {isAdmin && (
                <a href="/dashboard" className="header-dashboard-btn">
                  Dashboard
                </a>
              )}
              <a href="/add-listing" className="header-add-listing-btn">
                <span className="header-plus-icon">+</span>
                Add Listing
              </a>

              {/* Profile Dropdown */}
              <div className="header-profile-dropdown" ref={dropdownRef}>
                <button onClick={toggleDropdown} className="header-profile-btn">
                  <div className="header-profile-picture">
                    {getInitials(user?.firstName, user?.lastName)}
                  </div>
                  <span>{user?.firstName || 'Profile'}</span>
                  <span className="header-dropdown-arrow">{isDropdownOpen ? '▲' : '▼'}</span>
                </button>

                {isDropdownOpen && (
                  <div className="header-dropdown-menu">
                    <a href="/profile" className="header-dropdown-item">My Profile</a>
                    <a href="/my-listings" className="header-dropdown-item">My Listings</a>
                    <a href="/saved-listings" className="header-dropdown-item">Saved Listings</a>
                    <a href="/inbox" className="header-dropdown-item">
                      Inbox
                      {unreadCount > 0 && <span className="inbox-unread-dot" />}
                    </a>
                    <div className="header-dropdown-divider"></div>
                    <button onClick={handleLogout} className="header-dropdown-logout">
                      Logout
                    </button>
                  </div>
                )}
              </div>
            </div>
          ) : (
            <div className="header-auth-buttons">
              <a href="/login" className="header-login-btn">Login</a>
              <a href="/register" className="header-register-btn">Register</a>
            </div>
          )}
        </div>
      </div>
    </header>
  );
};

export default Header;

<style>{`
  .inbox-unread-dot {
    display: inline-block;
    width: 10px;
    height: 10px;
    background: orange;
    border-radius: 50%;
    margin-left: 8px;
    vertical-align: middle;
  }
`}</style>
