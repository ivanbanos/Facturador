
const GetCanastilla = async () => {
  try {
    const response = await fetch(window.SERVER_URL + "/api/Canastilla");

    if (response.status === 200) {
      return await response.json();
    }
    if (response.status === 403) {
      return "fail";
    }
    return "fail";
  } catch (error) {
    return "fail";
  }
};

export default GetCanastilla;
