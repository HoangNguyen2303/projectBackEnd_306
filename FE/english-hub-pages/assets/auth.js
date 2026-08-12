(function () {
  var API_URL = "http://localhost:5245";
  var AUTH_KEY = "edutrack_auth";

  function getAuth() {
    try {
      var raw = localStorage.getItem(AUTH_KEY);
      if (!raw) return null;
      var parsed = JSON.parse(raw);
      return parsed && parsed.token ? parsed : null;
    } catch (e) {
      return null;
    }
  }

  function setAuth(data) {
    localStorage.setItem(AUTH_KEY, JSON.stringify(data));
  }

  function clearAuth() {
    localStorage.removeItem(AUTH_KEY);
  }

  function roleToPrototypeRole(backendRole) {
    var map = { Admin: "admin", Teacher: "teacher", Student: "student" };
    return map[backendRole] || "student";
  }

  function post(path, body) {
    return fetch(API_URL + path, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    })
      .then(function (res) {
        return res.json().then(function (json) {
          return { res: res, json: json };
        });
      })
      .then(function (result) {
        var json = result.json;
        if (!json.success || json.data === null) {
          throw new Error(json.message || "Đã có lỗi xảy ra.");
        }
        return json.data;
      });
  }

  function apiLogin(email, password) {
    return post("/api/auth/login", { email: email, password: password });
  }

  function apiRegister(fullName, email, password) {
    return post("/api/auth/register", { fullName: fullName, email: email, password: password });
  }

  window.EduTrackAuth = {
    getAuth: getAuth,
    setAuth: setAuth,
    clearAuth: clearAuth,
    apiLogin: apiLogin,
    apiRegister: apiRegister,
    roleToPrototypeRole: roleToPrototypeRole,
  };
})();
