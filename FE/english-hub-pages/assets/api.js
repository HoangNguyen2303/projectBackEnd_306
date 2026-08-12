(function () {
  var API_URL = "http://localhost:5245";

  function authHeaders() {
    var auth = window.EduTrackAuth.getAuth();
    return auth ? { Authorization: "Bearer " + auth.token } : {};
  }

  function request(method, path, body) {
    return fetch(API_URL + path, {
      method: method,
      headers: Object.assign({ "Content-Type": "application/json" }, authHeaders()),
      body: body !== undefined ? JSON.stringify(body) : undefined,
    }).then(function (res) {
      if (res.status === 401) {
        window.EduTrackAuth.clearAuth();
        window.location.href = "./01-login.html";
        return new Promise(function () {});
      }
      if (res.status === 204) return null;
      return res.json().then(function (json) {
        if (!json.success) {
          throw new Error(json.message || "Đã có lỗi xảy ra.");
        }
        return json.data;
      });
    });
  }

  window.EduTrackApi = {
    get: function (path) { return request("GET", path); },
    post: function (path, body) { return request("POST", path, body); },
    put: function (path, body) { return request("PUT", path, body); },
    del: function (path) { return request("DELETE", path); },
  };
})();
